namespace Shuttle.Access.WebApi;

public static class SessionExtensions
{
    extension(Query.Session session)
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
                TokenHash = session.TokenHash,
                Application = session.Application,
                Permissions = session.Permissions.Select(item => new Contracts.v1.Session.Permission
                {
                    Id = item.Id,
                    Name = item.Name,
                    TenantId = item.TenantId
                }).ToList()
            };
        }
    }
}