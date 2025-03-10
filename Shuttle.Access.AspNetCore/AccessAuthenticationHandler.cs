using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAccessService _accessService;
    public static readonly string AuthenticationScheme = "Shuttle.Access";
    public const string SessionTokenClaimType = "http://shuttle.org/claims/session/token";
    public static readonly Regex TokenExpression = new(@"token\s*=\s*(?<token>[0-9a-fA-F-]{36})", RegexOptions.IgnoreCase);
    private const string Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2";

    public AccessAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IAccessService accessService) : base(options, logger, encoder)
    {
        _accessService = Guard.AgainstNull(accessService);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await Task.CompletedTask;

        var header = Request.Headers["Authorization"].FirstOrDefault();

        if (header == null)
        {
            return AuthenticateResult.NoResult();
        }

        if (!header.StartsWith("Shuttle.Access ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var match = TokenExpression.Match(header["Shuttle.Access ".Length..].Trim());

        if (!match.Success ||
            !Guid.TryParse(match.Groups["token"].Value, out var sessionToken))
        {
            return AuthenticateResult.Fail(Resources.InvalidAuthenticationHeader);
        }

        var session = await _accessService.FindSessionAsync(sessionToken);

        if (session == null)
        {
            return AuthenticateResult.Fail(Resources.InvalidAuthenticationHeader);
        }

        Context.SetPrincipalAccessSessionToken(sessionToken);

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, session.IdentityName),
            new(ClaimTypes.Name, session.IdentityName),
            new(nameof(session.IdentityName), session.IdentityName),
            new(SessionTokenClaimType, $"{session.Token:D}")
        ];

        return AuthenticateResult.Success(new(new(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authenticateResult = await HandleAuthenticateOnceAsync();

        if (authenticateResult.Succeeded)
        {
            return;
        }
        
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "Shuttle.Access";

        var problemDetails = new ProblemDetails
        {
            Type = Type,
            Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized),
            Status = StatusCodes.Status401Unauthorized,
            Detail = authenticateResult.Failure?.Message
        };

        await Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null, "application/problem+json");
    }
}