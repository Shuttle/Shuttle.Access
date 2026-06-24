using Shuttle.Access.Query;

namespace Shuttle.Access.WebApi;

public static class SessionExtensions
{
    extension(Session session)
    {
        public Contracts.v1.Session Map()
        {
            ArgumentNullException.ThrowIfNull(session);

            return new()
            {
                Id = session.Id,
                IdentityId = session.IdentityId,
                IdentityName = session.IdentityName,
                IdentityDescription = session.IdentityDescription,
                DateRegistered = session.DateRegistered,
                ExpiryDate = session.ExpiryDate,
                Permissions = session.Permissions.Select(item => new Contracts.v1.Session.SessionPermission
                {
                    Id = item.Id,
                    Name = item.Name,
                    TenantId = item.TenantId
                }).ToList(),
                Tokens = session.Tokens.Select(item => new Contracts.v1.Session.SessionToken
                {
                    Id = item.Id,
                    DateRegistered = item.DateRegistered,
                    ExpiryDate = item.ExpiryDate,
                    TokenHash = item.TokenHash,
                    Application = item.Application
                }).ToList()
            };
        }
    }
}