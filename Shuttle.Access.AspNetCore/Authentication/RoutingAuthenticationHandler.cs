using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore.Authentication;

public class RoutingAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILogger _logger;
    private readonly AccessOptions _accessOptions;
    public const string AuthenticationScheme = "Routing";

    public RoutingAuthenticationHandler(IOptions<AccessOptions> accessOptions, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
        _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
        _logger = logger.CreateLogger(GetType());
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
                    ? SessionTokenAuthenticationHandler.AuthenticationScheme 
                    : null;
    }
}