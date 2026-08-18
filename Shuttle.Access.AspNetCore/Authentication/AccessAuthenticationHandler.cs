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

/// <summary>
///     The single authentication handler for Shuttle.Access.  It defers everything credential-specific to the
///     registered <see cref="ISessionResolver" /> and is responsible only for populating the
///     <see cref="ISessionContext" />, projecting the claims, and issuing the challenge.
/// </summary>
public class AccessAuthenticationHandler(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder, ISessionResolver sessionResolver, ISessionContext sessionContext)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    public const string AuthenticationScheme = "Shuttle.Access";

    private const string ProblemType = "https://tools.ietf.org/html/rfc9110#section-15.5.2";

    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly ILogger _logger = Guard.AgainstNull(loggerFactory).CreateLogger<AccessAuthenticationHandler>();
    private readonly ISessionContext _sessionContext = Guard.AgainstNull(sessionContext);
    private readonly ISessionResolver _sessionResolver = Guard.AgainstNull(sessionResolver);

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(header))
        {
            await _accessAuthorizationOptions.AuthorizationHeaderAvailable.InvokeAsync(new(header), Context.RequestAborted);
        }

        var result = await _sessionResolver.ResolveAsync(Context, Context.RequestAborted);

        if (!result.IsAuthenticated)
        {
            if (!result.IsFailure)
            {
                return AuthenticateResult.NoResult();
            }

            LogMessage.AuthenticationFailed(_logger, Scheme.Name, result.FailureReason);

            return AuthenticateResult.Fail(result.FailureReason);
        }

        _sessionContext.TenantId = result.TenantId;
        _sessionContext.Session = result.Session ?? Query.Session.Empty;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.IdentityName),
            new(ClaimTypes.Name, result.IdentityName),
            new(HttpContextExtensions.SessionTenantIdClaimType, $"{result.TenantId:D}")
        };

        if (result.Session != null)
        {
            claims.Add(new(HttpContextExtensions.SessionIdClaimType, $"{result.Session.Id:D}"));
        }

        if (result.SessionToken.HasValue)
        {
            claims.Add(new(HttpContextExtensions.SessionTokenClaimType, $"{result.SessionToken.Value:D}"));
        }

        return AuthenticateResult.Success(new(new(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (Response.HasStarted)
        {
            return;
        }

        var authenticateResult = await HandleAuthenticateOnceAsync();

        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = $"Shuttle.Access realm=\"{_accessAuthorizationOptions.Realm}\", token=\"GUID\"; Bearer realm=\"{_accessAuthorizationOptions.Realm}\"";

        // A challenge can be raised by AccessAuthorizationMiddleware after authentication has already succeeded,
        // when the authenticated identity has no active session. That is not an invalid-token condition, so it
        // must not be reported as one.
        var detail = authenticateResult.Succeeded
            ? Access.Resources.NoActiveSession
            : authenticateResult.Failure?.Message ?? Access.Resources.InvalidAuthorizationHeader;

        LogMessage.AuthenticationFailed(_logger, Scheme.Name, detail);

        await Response.WriteAsJsonAsync(new ProblemDetails
        {
            Type = ProblemType,
            Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized),
            Status = StatusCodes.Status401Unauthorized,
            Detail = detail
        }, (JsonSerializerOptions?)null, "application/problem+json");
    }
}
