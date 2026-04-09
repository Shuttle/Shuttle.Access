using Shuttle.Contract;

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
                TenantId = session.TenantId,
                IdentityId = session.IdentityId,
                IdentityName = session.IdentityName,
                DateRegistered = session.DateRegistered,
                ExpiryDate = session.ExpiryDate,
                TokenHash = session.TokenHash,
                Permissions = session.Permissions.Select(item => new Contracts.v1.Permission
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Status = (int)item.Status,
                    StatusName = item.Status.ToString()
                }).ToList()
            };
        }
    }
}