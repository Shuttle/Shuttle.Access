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

namespace Shuttle.Access.AspNetCore.Authentication;

public class SessionTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISessionService sessionService)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2";
    public static readonly string AuthenticationScheme = "Shuttle.Access";
    public static readonly Regex TokenExpression = new(@"token\s*=\s*(?<token>[0-9a-fA-F-]{36})", RegexOptions.IgnoreCase);
    private readonly ISessionService _sessionService = Guard.AgainstNull(sessionService);

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers["Authorization"].FirstOrDefault();

        if (header == null || !header.StartsWith("Shuttle.Access ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var match = TokenExpression.Match(header["Shuttle.Access ".Length..].Trim());

        if (!match.Success ||
            !Guid.TryParse(match.Groups["token"].Value, out var sessionToken))
        {
            return AuthenticateResult.Fail(Access.Resources.InvalidAuthenticationHeader);
        }

        var session = await _sessionService.FindAsync(sessionToken);

        if (session == null)
        {
            return AuthenticateResult.Fail(Access.Resources.InvalidAuthenticationHeader);
        }

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, session.IdentityName),
            new(ClaimTypes.Name, session.IdentityName),
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

        await Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null, "application/problem+json");
    }
}