using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationMiddleware : IMiddleware
{
    public static readonly string AuthorizationScheme = "Shuttle.Access";
    private static readonly Regex TokenExpression = new(@"token\s*=\s*(?<token>[0-9a-fA-F-]{36})", RegexOptions.IgnoreCase);
    private readonly IAccessService _accessService;
    private readonly StringValues _wwwAuthenticate;


    public AccessAuthorizationMiddleware(IOptions<AccessOptions> accessOptions, IAccessService accessService)
    {
        var options = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);

        _accessService = Guard.AgainstNull(accessService);

        _wwwAuthenticate = $"Shuttle.Access realm=\"{options.Realm}\", token=\"GUID\"; Bearer realm=\"{options.Realm}\"";
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();

        var permissionRequirement = endpoint?.Metadata.GetMetadata<AccessPermissionRequirement>();
        var sessionRequirement = endpoint?.Metadata.GetMetadata<AccessSessionRequirement>();

        if (permissionRequirement == null && sessionRequirement == null)
        {
            await next(context);

            return;
        }

        var header = context.Request.Headers["Authorization"].FirstOrDefault();

        if (header == null)
        {
            Unauthorized(context);
            return;
        }

        if (!header.StartsWith("Shuttle.Access ", StringComparison.OrdinalIgnoreCase))
        {
            Unauthorized(context);
            return;
        }

        var match = TokenExpression.Match(header["Shuttle.Access ".Length..].Trim());

        if (!match.Success)
        {
            Unauthorized(context);
            return;
        }

        if (!Guid.TryParse(match.Groups["token"].Value, out var sessionToken))
        {
            Unauthorized(context);
            return;
        }

        if (!await _accessService.ContainsAsync(sessionToken))
        {
            Unauthorized(context);
            return;
        }

        context.User = new(new ClaimsIdentity([new(ClaimTypes.Name, "AuthenticatedUser"), new("scheme", AuthorizationScheme)], AuthorizationScheme));

        if (permissionRequirement != null &&
            !await _accessService.HasPermissionAsync(sessionToken, permissionRequirement.Permission))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            return;
        }

        await next(context);
    }

    private void Unauthorized(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.Append("WWW-Authenticate", _wwwAuthenticate);
    }
}