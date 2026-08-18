using Shuttle.Access.Query;

namespace Shuttle.Access.AspNetCore;

internal static class SessionMapper
{
    public static Session Map(WebApi.Contracts.v1.Session session)
    {
        return new()
        {
            Id = session.Id,
            IdentityId = session.IdentityId,
            IdentityName = session.IdentityName,
            IdentityDescription = session.IdentityDescription,
            DateRegistered = session.DateRegistered,
            ExpiryDate = session.ExpiryDate,
            Permissions = session.Permissions.Select(item => new Session.SessionPermission
            {
                Id = item.Id,
                Name = item.Name,
                TenantId = item.TenantId
            }).ToList(),
            Tokens = session.Tokens.Select(item => new Session.SessionToken
            {
                Id = item.Id,
                TokenHash = item.TokenHash,
                Application = item.Application,
                DateRegistered = item.DateRegistered,
                ExpiryDate = item.ExpiryDate
            }).ToList()
        };
    }
}
