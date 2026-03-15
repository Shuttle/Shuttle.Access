using Microsoft.Extensions.Logging;

namespace Shuttle.Access.RestClient;

public static class LogMessage
{
    private static readonly Action<ILogger, string, string, Exception?> SessionUnavailableDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new(1000, nameof(SessionUnavailable)), "Could not find a '{IdentifierType}' session for '{Identifier}'.");

    private static readonly Action<ILogger, string, Guid, Exception?> SessionAvailableDelegate =
        LoggerMessage.Define<string, Guid>(LogLevel.Debug, new(1001, nameof(SessionAvailable)), "Found a session for '{IdentifierName}' in tenant id '{TenantId}'.");

    public static void SessionUnavailable(ILogger logger, string identifierType, string identifier) =>
        SessionUnavailableDelegate(logger, identifierType, identifier, null);

    public static void SessionAvailable(ILogger logger, string identifierName, Guid tenantId) =>
        SessionAvailableDelegate(logger, identifierName, tenantId, null);
}
