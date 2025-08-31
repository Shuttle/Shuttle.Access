using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi;

public class LoggingHostedService(IOptions<AccessOptions> accessOptions, IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, ILogger<LoggingHostedService> logger) : IHostedService
{
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly ILogger<LoggingHostedService> _logger = Guard.AgainstNull(logger);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _accessAuthorizationOptions.SessionAvailable += OnSessionAvailable;
        _accessAuthorizationOptions.SessionUnavailable += OnSessionUnavailable;

        if (_accessOptions.AuthorizationHeaderLoggingEnabled)
        {
            _accessAuthorizationOptions.AuthorizationHeaderAvailable += OnAuthorizationHeaderAvailable;
        }

        await Task.CompletedTask;
    }

    private Task OnAuthorizationHeaderAvailable(AuthorizationHeaderAvailableEventArgs args)
    {
        _logger.LogDebug("[Authorization]: header = '{Header}'", args.Value);

        return Task.CompletedTask;
    }

    private Task OnSessionUnavailable(SessionUnavailableEventArgs args)
    {
        _logger.LogDebug("[Session/unavailable]: identifier type = '{IdentifierType}' / identifier = '{Identifier}'", args.IdentifierType, args.Identifier);

        return Task.CompletedTask;
    }

    private Task OnSessionAvailable(SessionAvailableEventArgs args)
    {
        _logger.LogDebug("[Session/available]: identity name = '{IdentityName}' / identity id = '{IdentityId}' / expiry date = '{ExpiryDate}'", args.Session.IdentityName, args.Session.IdentityId, args.Session.ExpiryDate.ToString("O"));

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _accessAuthorizationOptions.SessionAvailable -= OnSessionAvailable;
        _accessAuthorizationOptions.SessionUnavailable -= OnSessionUnavailable;

        if (_accessOptions.AuthorizationHeaderLoggingEnabled)
        {
            _accessAuthorizationOptions.AuthorizationHeaderAvailable -= OnAuthorizationHeaderAvailable;
        }

        await Task.CompletedTask;
    }
}