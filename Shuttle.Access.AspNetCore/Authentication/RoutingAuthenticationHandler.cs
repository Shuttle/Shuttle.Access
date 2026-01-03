using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore.Authentication;

public class RoutingAuthenticationHandler(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, loggerFactory, encoder)
{
    public const string AuthenticationScheme = "Routing";
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);

    private async ValueTask<string?> GetAuthenticationSchemeAsync()
    {
        var header = Request.Headers["Authorization"].FirstOrDefault();

        if (header == null)
        {
            return null;
        }

        await _accessAuthorizationOptions.AuthorizationHeaderAvailable.InvokeAsync(new(header));

        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? JwtBearerAuthenticationHandler.AuthenticationScheme
            : header.StartsWith("Shuttle.Access ", StringComparison.OrdinalIgnoreCase)
                ? SessionTokenAuthenticationHandler.AuthenticationScheme
                : null;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var scheme = await GetAuthenticationSchemeAsync();

        return string.IsNullOrWhiteSpace(scheme) ? AuthenticateResult.NoResult() : await Context.AuthenticateAsync(scheme);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var scheme = await GetAuthenticationSchemeAsync();

        if (string.IsNullOrWhiteSpace(scheme))
        {
            return;
        }

        await Context.ChallengeAsync(scheme);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        var scheme = await GetAuthenticationSchemeAsync();

        if (string.IsNullOrWhiteSpace(scheme))
        {
            return;
        }

        await Context.ForbidAsync(scheme);
    }
}