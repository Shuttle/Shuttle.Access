using Microsoft.Extensions.Logging;

namespace Shuttle.Access.RestClient;

public static class LogMessage
{
    private static readonly Action<ILogger, string, string, Exception?> SessionUnavailableDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new(1000, nameof(SessionUnavailable)), "Could not find a '{IdentifierType}' session for '{IdentifierName}'.");

    private static readonly Action<ILogger, string, string, Exception?> SessionAvailableDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new(1001, nameof(SessionUnavailable)), "Found a session for '{IdentifierName}' in tenant '{TenantName}'.");

    public static void SessionUnavailable(ILogger logger, string identifierType, string identifierName) =>
        SessionUnavailableDelegate(logger, identifierType, identifierName, null);

    public static void SessionAvailable(ILogger logger, string identifierName, string tenantName) =>
        SessionAvailableDelegate(logger, identifierName, tenantName, null);
}
