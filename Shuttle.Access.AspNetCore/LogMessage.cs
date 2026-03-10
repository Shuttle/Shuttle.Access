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

    private static readonly Action<ILogger, Guid, Guid?, Exception?> NoActiveSessionDelegate =
        LoggerMessage.Define<Guid, Guid?>(LogLevel.Debug, new(1003, nameof(NoActiveSession)), "No active session found for identity '{IdentityId}' and tenant '{TenantId}'.");

    private static readonly Action<ILogger, string, string, string, Exception?> PermissionDeniedDelegate =
        LoggerMessage.Define<string, string, string>(LogLevel.Debug, new(1004, nameof(PermissionDenied)), "Identity '{IdentityName}' in tenant '{TenantName}' does not have access to permission '{Permission}'.");

    public static void JwtIssuerOptionsUnavailable(ILogger logger, string jsonWebToken) =>
        JwtIssuerOptionsUnavailableDelegate(logger, jsonWebToken, null);
    
    public static void JwtIssuerOptionsAvailable(ILogger logger, string jsonWebToken) =>
        JwtIssuerOptionsAvailableDelegate(logger, jsonWebToken, null);

    public static void AuthenticationFailed(ILogger logger, string scheme, string reason) =>
        AuthenticationFailedDelegate(logger, scheme, reason, null);

    public static void NoActiveSession(ILogger logger, Guid identityId, Guid? tenantId) =>
        NoActiveSessionDelegate(logger, identityId, tenantId, null);

    public static void PermissionDenied(ILogger logger, string identityName, string tenantName, string permission) =>
        PermissionDeniedDelegate(logger, identityName, tenantName, permission, null);
}
