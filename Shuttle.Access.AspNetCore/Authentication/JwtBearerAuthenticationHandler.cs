using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public class JwtBearerAuthenticationHandler(IOptions<AccessOptions> accessOptions, IOptionsMonitor<AuthenticationSchemeOptions> options, ISessionService sessionService, ILoggerFactory loggerFactory, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    private const string Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2";
    public static readonly string AuthenticationScheme = "Bearer";
    private readonly ILogger _logger = Guard.AgainstNull(loggerFactory).CreateLogger<JwtBearerAuthenticationHandler>();

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationHeader = Request.Headers.Authorization.FirstOrDefault();

        if (authorizationHeader == null)
        {
            return AuthenticateResult.NoResult();
        }

        var tenantId = Request.GetTenantId(_logger, accessOptions.Value.SystemTenantId);

        if (!tenantId.HasValue)
        {
            return await Response.GetTenantIdInvalidAuthenticateResultAsync();
        }

        var session = await sessionService.GetSelfAsync();

        if (session == null)
        {
            return AuthenticateResult.Fail(Access.Resources.InvalidAuthorizationHeader);
        }

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, session.IdentityName),
            new(ClaimTypes.Name, session.IdentityName),
            new(HttpContextExtensions.SessionTenantIdClaimType, $"{tenantId:D}"),
            new(HttpContextExtensions.SessionIdentityIdClaimType, $"{session.IdentityId:D}")
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

        LogMessage.AuthenticationFailed(_logger, AuthenticationScheme, authenticateResult.Failure?.Message ?? "Unknown authentication failure.");

        await Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null, "application/problem+json");
    }
}