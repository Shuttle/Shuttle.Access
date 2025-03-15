using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.OAuth;

namespace Shuttle.Access.WebApi.Authentication;

public class JwtBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IJwtService _jwtService;
    public static readonly string AuthenticationScheme = "Bearer";
    private const string Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2";

    public JwtBearerAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, IJwtService jwtService, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
        _jwtService = Guard.AgainstNull(jwtService);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers["Authorization"].FirstOrDefault();

        if (header == null)
        {
            return AuthenticateResult.NoResult();
        }

        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
            header.Length < 8)
        {
            return AuthenticateResult.NoResult();
        }

        var token = header[7..];

        if (!await _jwtService.IsValidAsync(token))
        {
            return AuthenticateResult.Fail(Resources.InvalidAuthenticationHeader);
        }

        var identityName = await _jwtService.GetIdentityNameAsync(token);

        if (string.IsNullOrWhiteSpace(identityName))
        {
            return AuthenticateResult.Fail(Resources.IdentityNameClaimNotFound);
        }

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, identityName),
            new(ClaimTypes.Name, identityName),
            new(nameof(Session.IdentityName), identityName),
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