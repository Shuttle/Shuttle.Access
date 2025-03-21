using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Shuttle.Access.WebApi;

public class RoutingAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "Routing";

    public RoutingAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var scheme = GetAuthenticationScheme();

        return string.IsNullOrWhiteSpace(scheme) ? AuthenticateResult.NoResult() : await Context.AuthenticateAsync(scheme);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var scheme = GetAuthenticationScheme();

        if (string.IsNullOrWhiteSpace(scheme))
        {
            return;
        }

        await Context.ChallengeAsync(scheme);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        var scheme = GetAuthenticationScheme();
        
        if (string.IsNullOrWhiteSpace(scheme))
        {
            return;
        }

        await Context.ForbidAsync(scheme);
    }

    private string? GetAuthenticationScheme()
    {
        var header = Request.Headers["Authorization"].FirstOrDefault();

        return header == null 
            ? null 
            : header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) 
                ? JwtBearerAuthenticationHandler.AuthenticationScheme 
                : header.StartsWith("Shuttle.Access ", StringComparison.OrdinalIgnoreCase) 
                    ? AccessAuthenticationHandler.AuthenticationScheme 
                    : null;
    }
}