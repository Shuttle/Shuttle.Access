using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public static class HttpContextExtensions
{
    public const string SessionIdentityIdClaimType = "http://shuttle.org/claims/session/identity-id";
    public const string SessionTenantIdClaimType = "http://shuttle.org/claims/session/tenant-id";

    extension(HttpContext httpContext)
    {
        public Guid? FindIdentityId()
        {
            var value = Guard.AgainstNull(httpContext).User.Claims.FirstOrDefault(claim => claim.Type == SessionIdentityIdClaimType)?.Value ?? string.Empty;

            return string.IsNullOrWhiteSpace(value) || !Guid.TryParse(value, out var identityId) ? null : identityId;
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
    }
}