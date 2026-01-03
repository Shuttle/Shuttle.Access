using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi;

public class LoggingHostedService(IOptions<AccessAuthorizationOptions> accessAuthorizationOptions, ILogger<LoggingHostedService> logger) : IHostedService
{
    private readonly AccessAuthorizationOptions _accessAuthorizationOptions = Guard.AgainstNull(Guard.AgainstNull(accessAuthorizationOptions).Value);
    private readonly ILogger<LoggingHostedService> _logger = Guard.AgainstNull(logger);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _accessAuthorizationOptions.SessionAvailable += OnSessionAvailable;
        _accessAuthorizationOptions.SessionUnavailable += OnSessionUnavailable;
        _accessAuthorizationOptions.JwtIssuerOptionsAvailable += OnJwtIssuerOptionsAvailable;
        _accessAuthorizationOptions.JwtIssuerOptionsUnavailable += OnJwtIssuerOptionsUnavailable;

        if (_accessAuthorizationOptions.InsecureModeEnabled)
        {
            _accessAuthorizationOptions.AuthorizationHeaderAvailable += OnAuthorizationHeaderAvailable;
        }

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _accessAuthorizationOptions.SessionAvailable -= OnSessionAvailable;
        _accessAuthorizationOptions.SessionUnavailable -= OnSessionUnavailable;
        _accessAuthorizationOptions.JwtIssuerOptionsAvailable -= OnJwtIssuerOptionsAvailable;
        _accessAuthorizationOptions.JwtIssuerOptionsUnavailable -= OnJwtIssuerOptionsUnavailable;

        if (_accessAuthorizationOptions.InsecureModeEnabled)
        {
            _accessAuthorizationOptions.AuthorizationHeaderAvailable -= OnAuthorizationHeaderAvailable;
        }

        await Task.CompletedTask;
    }

    private async Task OnAuthorizationHeaderAvailable(AuthorizationHeaderAvailableEventArgs eventArgs, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("[Authorization]: header = '{Header}'", eventArgs.Value);

        await Task.CompletedTask;
    }

    private async Task OnJwtIssuerOptionsAvailable(JwtIssuerOptionsAvailableEventArgs eventArgs, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("[JwtIssuerOptions/available]: issuer = '{Issuer}' / identity name claim types = '{IdentityNameClaimTypes}' / claims = '{Claims}'", eventArgs.JsonWebToken.Issuer, string.Join(',', eventArgs.IssuerOptions.IdentityNameClaimTypes), string.Join(',', eventArgs.JsonWebToken.Claims.Select(claim => $"'{claim.Type} = {claim.Value}'")));

        await Task.CompletedTask;
    }

    private async Task OnJwtIssuerOptionsUnavailable(JwtIssuerOptionsUnavailableEventArgs eventArgs, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("[JwtIssuerOptions/unavailable]: issuer = '{Issuer}'", eventArgs.JsonWebToken.Issuer);

        await Task.CompletedTask;
    }

    private async Task OnSessionAvailable(SessionAvailableEventArgs eventArgs, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("[Session/available]: identity name = '{IdentityName}' / identity id = '{IdentityId}' / expiry date = '{ExpiryDate}'", eventArgs.Session.IdentityName, eventArgs.Session.IdentityId, eventArgs.Session.ExpiryDate.ToString("O"));

        await Task.CompletedTask;
    }

    private async Task OnSessionUnavailable(SessionUnavailableEventArgs eventArgs, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("[Session/unavailable]: identifier type = '{IdentifierType}' / identifier = '{Identifier}'", eventArgs.IdentifierType, eventArgs.Identifier);

        await Task.CompletedTask;
    }
}