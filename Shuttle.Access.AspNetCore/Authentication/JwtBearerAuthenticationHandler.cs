using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Access.Query;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public class JwtBearerAuthenticationHandler(ILogger<JwtBearerAuthenticationHandler> logger, IOptions<AccessOptions> accessOptions, IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IOptionsMonitor<AuthenticationSchemeOptions> options, ISessionService sessionService, IJwtService jwtService, ILoggerFactory loggerFactory, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    private const string Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2";
    public static readonly string AuthenticationScheme = "Bearer";
    private readonly ILogger _logger = Guard.AgainstNull(loggerFactory).CreateLogger<JwtBearerAuthenticationHandler>();

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Session? session;
        List<Claim> claims;

        var tenantId = Request.GetTenantId(_logger, accessOptions.Value.SystemTenantId);

        if (!tenantId.HasValue)
        {
            return await Response.GetTenantIdInvalidAuthenticateResultAsync();
        }

        if (accessAuthorizationOptions.Value.PassThrough)
        {
            session = await sessionService.GetSelfAsync();

            if (session == null)
            {
                return AuthenticateResult.Fail(Access.Resources.InvalidAuthorizationHeader);
            }

            claims = [
                new(ClaimTypes.NameIdentifier, session.IdentityName),
                new(ClaimTypes.Name, session.IdentityName),
                new(HttpContextExtensions.SessionIdClaimType, $"{session.Id:D}"),
                new(HttpContextExtensions.SessionTenantIdClaimType, $"{tenantId:D}")
            ];

            return AuthenticateResult.Success(new(new(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name));
        }

        var authorizationHeader = Request.Headers.Authorization.FirstOrDefault();

        if (authorizationHeader == null)
        {
            return AuthenticateResult.NoResult();
        }

        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
            authorizationHeader.Length < 8)
        {
            LogMessage.InvalidAuthorizationHeader(logger, "Bearer");
            return AuthenticateResult.Fail(Access.Resources.InvalidAuthorizationHeader);
        }

        var token = authorizationHeader[7..];
        var identityName = await jwtService.GetIdentityNameAsync(token);

        if (string.IsNullOrWhiteSpace(identityName))
        {
            LogMessage.IdentityNameClaimNotFound(logger);
            return AuthenticateResult.Fail(Access.Resources.IdentityNameClaimNotFound);
        }

        var tokenValidationResult = await jwtService.ValidateTokenAsync(token);

        if (!tokenValidationResult.IsValid)
        {
            LogMessage.InvalidAuthorizationHeader(logger, "Bearer");

            var failureMessage = tokenValidationResult.Exception?.Message ?? Access.Resources.InvalidAuthorizationHeader;

            Response.StatusCode = StatusCodes.Status401Unauthorized;

            await Response.WriteAsJsonAsync(new ProblemDetails
            {
                Type = Type,
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = failureMessage
            });

            return AuthenticateResult.Fail(failureMessage);
        }

        session = await sessionService.FindAsync(new Session.Specification().WithIdentityName(identityName));

        claims =
        [
            new(ClaimTypes.NameIdentifier, identityName),
            new(ClaimTypes.Name, identityName),
            new(HttpContextExtensions.SessionTenantIdClaimType, $"{tenantId:D}")
        ];

        if (session != null)
        {
            claims.Add(new(HttpContextExtensions.SessionIdClaimType, $"{session.Id:D}"));
        }

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