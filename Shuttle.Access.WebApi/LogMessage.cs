namespace Shuttle.Access.WebApi;

public static class LogMessage
{
    private static readonly Action<ILogger, string, string, Exception?> RegisterSessionIdentityMismatchDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new(1000, nameof(RegisterSessionIdentityMismatch)), "The identity determined from the HTTP Context is '{Identity}' but the session registration request is for '{RequestedIdentity}'.");

    private static readonly Action<ILogger, string, Exception?> RegisterSessionUnauthorizedDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new(1001, nameof(RegisterSessionUnauthorized)), "Identity '{Identity}' requires permission 'access://sessions/register'.");

    private static readonly Action<ILogger, string, Exception?> JwtIssuerOptionsUnavailableDelegate =
        LoggerMessage.Define<string>(LogLevel.Debug, new(1002, nameof(JwtIssuerOptionsUnavailable)), "Could not find issuer options for JWT '{JsonWebToken}'.");

    private static readonly Action<ILogger, string, Exception?> JwtIssuerOptionsAvailableDelegate =
        LoggerMessage.Define<string>(LogLevel.Debug, new(1003, nameof(JwtIssuerOptionsAvailable)), "Found issuer options for JWT '{JsonWebToken}'.");

    private static readonly Action<ILogger, string, string, Exception?> JwtIdentityNameClaimFoundDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new(1004, nameof(JwtIdentityNameClaimFound)), "JWT identity name claim found: '{ClaimType}' = '{ClaimValue}'.");

    private static readonly Action<ILogger, string, Exception?> JwtIdentityNameClaimNotFoundDelegate =
        LoggerMessage.Define<string>(LogLevel.Warning, new(1005, nameof(JwtIdentityNameClaimNotFound)), "JWT identity name claim not found. Searched claim types: '{ClaimTypes}'.");

    public static void RegisterSessionIdentityMismatch(ILogger logger, string identity, string requestedIdentity) =>
        RegisterSessionIdentityMismatchDelegate(logger, identity, requestedIdentity, null);

    public static void RegisterSessionUnauthorized(ILogger logger, string identity) =>
        RegisterSessionUnauthorizedDelegate(logger, identity, null);

    public static void JwtIssuerOptionsUnavailable(ILogger logger, string jsonWebToken) =>
        JwtIssuerOptionsUnavailableDelegate(logger, jsonWebToken, null);

    public static void JwtIssuerOptionsAvailable(ILogger logger, string jsonWebToken) =>
        JwtIssuerOptionsAvailableDelegate(logger, jsonWebToken, null);

    public static void JwtIdentityNameClaimFound(ILogger logger, string claimType, string claimValue) =>
        JwtIdentityNameClaimFoundDelegate(logger, claimType, claimValue, null);

    public static void JwtIdentityNameClaimNotFound(ILogger logger, string claimTypes) =>
        JwtIdentityNameClaimNotFoundDelegate(logger, claimTypes, null);
}
