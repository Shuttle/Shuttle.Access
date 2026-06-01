namespace Shuttle.Access.WebApi;

public static class LogMessage
{
    private static readonly Action<ILogger, string, string, Exception?> RegisterSessionIdentityMismatchDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new(1000, nameof(RegisterSessionIdentityMismatch)), "The identity determined from the HTTP Context is '{Identity}' but the session registration request is for '{RequestedIdentity}'.");

    private static readonly Action<ILogger, string, Exception?> RegisterSessionUnauthorizedDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new(1001, nameof(RegisterSessionUnauthorized)), "Identity '{Identity}' requires permission 'access://sessions/register'.");

    private static readonly Action<ILogger, string, Exception?> InvalidAuthorizationHeaderDelegate =
        LoggerMessage.Define<string>(LogLevel.Trace, new(1002, nameof(InvalidAuthorizationHeader)), "{Message}");


    private static readonly Action<ILogger, Exception?> IdentityNameClaimNotFoundDelegate =
        LoggerMessage.Define(LogLevel.Trace, new(1003, nameof(IdentityNameClaimNotFound)), "Using pass-through.");

    public static void RegisterSessionIdentityMismatch(ILogger logger, string identity, string requestedIdentity) =>
        RegisterSessionIdentityMismatchDelegate(logger, identity, requestedIdentity, null);

    public static void RegisterSessionUnauthorized(ILogger logger, string identity) =>
        RegisterSessionUnauthorizedDelegate(logger, identity, null);

    public static void InvalidAuthorizationHeader(ILogger logger, string scheme) =>
        InvalidAuthorizationHeaderDelegate(logger, scheme, null);

    public static void IdentityNameClaimNotFound(ILogger logger) =>
        IdentityNameClaimNotFoundDelegate(logger, null);
}
