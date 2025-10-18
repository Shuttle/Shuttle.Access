using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Shuttle.Access.AspNetCore.Authentication;

public class JwtBearerAuthenticationHandler(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IOptionsMonitor<AuthenticationSchemeOptions> options, IJwtService jwtService, IContextSessionService contextSessionService, ISessionService sessionService, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2";
    public static readonly string AuthenticationScheme = "Bearer";
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly IContextSessionService _contextSessionService = Guard.AgainstNull(contextSessionService);
    private readonly IJwtService _jwtService = Guard.AgainstNull(jwtService);
    private readonly ISessionService _sessionService = Guard.AgainstNull(sessionService);

    private async Task<AuthenticateResult> GetContextAuthenticateResultAsync()
    {
        var session = await _contextSessionService.FindAsync();

        if (session == null)
        {
            return AuthenticateResult.Fail(Resources.ContextSessionNotFound);
        }

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, session.IdentityName),
            new(ClaimTypes.Name, session.IdentityName),
            new(HttpContextExtensions.SessionIdentityIdClaimType, $"{session.IdentityId:D}")
        ];

        return AuthenticateResult.Success(new(new(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name));
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers["Authorization"].FirstOrDefault();

        if (header == null)
        {
            return AuthenticateResult.NoResult();
        }

        if (_accessAuthorizationOptions.PassThrough)
        {
            return await GetContextAuthenticateResultAsync();
        }

        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
            header.Length < 8)
        {
            return AuthenticateResult.NoResult();
        }

        var token = header[7..];
        var identityName = await _jwtService.GetIdentityNameAsync(token);

        if (string.IsNullOrWhiteSpace(identityName))
        {
            return AuthenticateResult.Fail(Access.Resources.IdentityNameClaimNotFound);
        }

        var tokenValidationResult = await _jwtService.ValidateTokenAsync(token);

        if (!tokenValidationResult.IsValid)
        {
            var failureMessage = (tokenValidationResult.Exception ?? new AuthenticationException(Access.Resources.InvalidAuthenticationHeader)).Message;

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

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, identityName),
            new(ClaimTypes.Name, identityName)
        ];

        var session = await _sessionService.FindAsync(identityName);

        if (session != null)
        {
            claims.Add(new(HttpContextExtensions.SessionIdentityIdClaimType, $"{session.IdentityId:D}"));
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

        await Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null, "application/problem+json");
    }
}