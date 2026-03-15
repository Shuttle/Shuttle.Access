namespace Shuttle.Access.WebApi;

public static class LogMessage
{
    private static readonly Action<ILogger, string, string, Exception?> RegisterSessionIdentityMismatchDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new(1000, nameof(RegisterSessionIdentityMismatch)), "The identity determined from the HTTP Context is '{Identity}' but the session registration request is for '{RequestedIdentity}'.");

    private static readonly Action<ILogger, string, Exception?> RegisterSessionUnauthorizedDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new(1001, nameof(RegisterSessionUnauthorized)), "Identity '{Identity}' requires permission 'access://sessions/register'.");

    public static void RegisterSessionIdentityMismatch(ILogger logger, string identity, string requestedIdentity) =>
        RegisterSessionIdentityMismatchDelegate(logger, identity, requestedIdentity, null);

    public static void RegisterSessionUnauthorized(ILogger logger, string identity) =>
        RegisterSessionUnauthorizedDelegate(logger, identity, null);
}
