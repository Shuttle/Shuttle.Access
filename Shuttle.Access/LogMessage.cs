using Microsoft.Extensions.Logging;

namespace Shuttle.Access;

public static class LogMessage
{
    private static readonly Action<ILogger, string, string, Exception?> SessionUnavailableDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new(1000, nameof(SessionUnavailable)), "Could not find a '{IdentifierType}' session for '{Identifier}'.");

    private static readonly Action<ILogger, string, bool, Exception?> SessionAvailableDelegate =
        LoggerMessage.Define<string, bool>(LogLevel.Debug, new(1001, nameof(SessionAvailable)), "Found a session for '{IdentifierName}'.  Cache hit = {CacheHit}.");

    public static void SessionUnavailable(ILogger logger, string identifierType, string identifier) =>
        SessionUnavailableDelegate(logger, identifierType, identifier, null);

    public static void SessionAvailable(ILogger logger, string identifierName, bool cacheHit) =>
        SessionAvailableDelegate(logger, identifierName, cacheHit, null);
}
