using Microsoft.Extensions.Logging;

namespace Shuttle.Access.AspNetCore;

public static class LogMessage
{
    private static readonly Action<ILogger, string, Exception?> JwtIssuerOptionsUnavailableDelegate =
        LoggerMessage.Define<string>(LogLevel.Debug, new(1000, nameof(JwtIssuerOptionsUnavailable)), "Could not find issuer options for JWT '{JsonWebToken}'.");

    private static readonly Action<ILogger, string, Exception?> JwtIssuerOptionsAvailableDelegate =
        LoggerMessage.Define<string>(LogLevel.Debug, new(1001, nameof(JwtIssuerOptionsAvailable)), "Find issuer options for JWT '{JsonWebToken}'.");

    private static readonly Action<ILogger, string, string, Exception?> AuthenticationFailedDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Debug, new(1002, nameof(AuthenticationFailed)), "Authentication failed for scheme '{AuthenticationScheme}'. Reason: {Reason}");

    private static readonly Action<ILogger, Guid, Guid?, Exception?> SessionUnavailableDelegate =
        LoggerMessage.Define<Guid, Guid?>(LogLevel.Debug, new(1003, nameof(SessionUnavailable)), "No active session found for identity '{IdentityId}' and tenant '{TenantId}'.");

    private static readonly Action<ILogger, string, string, string, Exception?> PermissionDeniedDelegate =
        LoggerMessage.Define<string, string, string>(LogLevel.Trace, new(1004, nameof(PermissionDenied)), "Identity '{IdentityName}' in tenant '{TenantName}' does not have access to permission '{Permission}'.");

    private static readonly Action<ILogger, string, string, Exception?> TenantIdHeaderDelegate =
        LoggerMessage.Define<string, string>(LogLevel.Trace, new(1005, nameof(TenantIdHeader)), "{Message}.  Tenant id header = '{TenantIdHeader}'.");

    private static readonly Action<ILogger, string, Guid, Exception?> TenantIdDelegate =
        LoggerMessage.Define<string, Guid>(LogLevel.Trace, new(1006, nameof(TenantId)), "{Message}.  Tenant id = '{TenantId}'.");

    private static readonly Action<ILogger, string, Exception?> InvalidTenantIdHeaderDelegate =
        LoggerMessage.Define<string>(LogLevel.Trace, new(1007, nameof(InvalidTenantIdHeader)), "{Message}");

    private static readonly Action<ILogger, Exception?> PassThroughDelegate =
        LoggerMessage.Define(LogLevel.Trace, new(1008, nameof(InvalidTenantIdHeader)), "Using pass-through.");

    private static readonly Action<ILogger, string, Exception?> InvalidAuthorizationHeaderDelegate =
        LoggerMessage.Define<string>(LogLevel.Trace, new(1009, nameof(InvalidAuthorizationHeader)), "{Message}");

    private static readonly Action<ILogger, Exception?> IdentityNameClaimNotFoundDelegate =
        LoggerMessage.Define(LogLevel.Trace, new(1008, nameof(IdentityNameClaimNotFound)), "Using pass-through.");

    public static void JwtIssuerOptionsUnavailable(ILogger logger, string jsonWebToken) =>
        JwtIssuerOptionsUnavailableDelegate(logger, jsonWebToken, null);
    
    public static void JwtIssuerOptionsAvailable(ILogger logger, string jsonWebToken) =>
        JwtIssuerOptionsAvailableDelegate(logger, jsonWebToken, null);

    public static void AuthenticationFailed(ILogger logger, string scheme, string reason) =>
        AuthenticationFailedDelegate(logger, scheme, reason, null);

    public static void SessionUnavailable(ILogger logger, Guid identityId, Guid? tenantId) =>
        SessionUnavailableDelegate(logger, identityId, tenantId, null);

    public static void PermissionDenied(ILogger logger, string identityName, string tenantName, string permission) =>
        PermissionDeniedDelegate(logger, identityName, tenantName, permission, null);

    public static void TenantIdHeader(ILogger logger, string message, string tenantIdHeader) =>
        TenantIdHeaderDelegate(logger, message, tenantIdHeader, null);

    public static void TenantId(ILogger logger, string message, Guid tenantId) =>
        TenantIdDelegate(logger, message, tenantId, null);

    public static void InvalidTenantIdHeader(ILogger logger, string message) =>
        InvalidTenantIdHeaderDelegate(logger, message, null);

    public static void PassThrough(ILogger logger) =>
        PassThroughDelegate(logger, null);

    public static void InvalidAuthorizationHeader(ILogger logger, string scheme) =>
        InvalidAuthorizationHeaderDelegate(logger, scheme, null);

    public static void IdentityNameClaimNotFound(ILogger logger) =>
        IdentityNameClaimNotFoundDelegate(logger, null);
}
