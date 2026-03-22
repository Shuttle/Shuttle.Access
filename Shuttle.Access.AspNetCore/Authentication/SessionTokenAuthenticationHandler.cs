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

public class SessionTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder, ISessionService sessionService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    private const string Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2";
    public static readonly string AuthenticationScheme = "Shuttle.Access";
    public static readonly Regex TokenExpression = new(@"token\s*=\s*(?<token>[0-9a-fA-F-]{36})", RegexOptions.IgnoreCase);
    private readonly ISessionService _sessionService = Guard.AgainstNull(sessionService);
    private readonly ILogger _logger = Guard.AgainstNull(loggerFactory).CreateLogger<JwtBearerAuthenticationHandler>();

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.FirstOrDefault();

        if (header == null || !header.StartsWith("Shuttle.Access ", StringComparison.OrdinalIgnoreCase))
        {
            LogMessage.AuthenticationFailed(_logger, "Shuttle.Access", "The 'Authorization' header does not start with 'Shuttle.Access'.");
            return AuthenticateResult.NoResult();
        }

        var match = TokenExpression.Match(header["Shuttle.Access ".Length..].Trim());

        if (!match.Success ||
            !Guid.TryParse(match.Groups["token"].Value, out var sessionToken))
        {
            LogMessage.AuthenticationFailed(_logger, "Shuttle.Access", $"The 'token' value '{match.Groups["token"].Value}' provided is not a valid GUID.");
            return AuthenticateResult.Fail(Access.Resources.InvalidAuthorizationHeader);
        }

        var session = await _sessionService.FindAsync(new Query.Session.Specification().WithToken(sessionToken));

        if (session == null)
        {
            return AuthenticateResult.Fail(Access.Resources.InvalidAuthorizationHeader);
        }

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, session.IdentityName),
            new(ClaimTypes.Name, session.IdentityName),
            new(HttpContextExtensions.SessionIdentityIdClaimType, $"{session.IdentityId:D}"),
            new(HttpContextExtensions.SessionTenantIdClaimType, $"{session.TenantId:D}"),
            new(HttpContextExtensions.SessionTokenClaimType, $"{sessionToken:D}")
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