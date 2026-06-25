using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public static class HttpContextExtensions
{
    public const string SessionIdClaimType = "http://shuttle.org/claims/session/id";
    public const string SessionTenantIdClaimType = "http://shuttle.org/claims/session/tenant-id";
    public const string SessionTokenClaimType = "http://shuttle.org/claims/session/token";

    extension(HttpContext httpContext)
    {
        public Guid? FindSessionsId()
        {
            var value = Guard.AgainstNull(httpContext).User.Claims.FirstOrDefault(claim => claim.Type == SessionIdClaimType)?.Value ?? string.Empty;

            return string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var sessionId) ? null : sessionId;
        }

        public string? FindIdentityName()
        {
            var value = Guard.AgainstNull(httpContext).User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public Guid? FindTenantId()
        {
            var value = Guard.AgainstNull(httpContext).User.Claims.FirstOrDefault(claim => claim.Type == SessionTenantIdClaimType)?.Value ?? string.Empty;

            return Guid.TryParse(value, out var result) ? result : null;
        }

        public Guid? FindToken()
        {
            var value = Guard.AgainstNull(httpContext).User.Claims.FirstOrDefault(claim => claim.Type == SessionTokenClaimType)?.Value ?? string.Empty;

            return Guid.TryParse(value, out var result) ? result : null;
        }
    }
}