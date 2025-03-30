using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public static class HttpContextExtensions
{
    public const string SessionIdentityIdClaimType = "http://shuttle.org/claims/session/identity-id";

    public static string? GetIdentityName(this HttpContext context)
    {
        var identityNameValue = Guard.AgainstNull(context).User.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

        return string.IsNullOrWhiteSpace(identityNameValue) ? null : identityNameValue;
    }

    public static Guid? GetIdentityId(this HttpContext context)
    {
        var identityIdValue = Guard.AgainstNull(context).User.Claims.FirstOrDefault(claim => claim.Type == SessionIdentityIdClaimType)?.Value ?? string.Empty;

        return string.IsNullOrWhiteSpace(identityIdValue) || !Guid.TryParse(identityIdValue, out var identityId) ? null : identityId;
    }
}