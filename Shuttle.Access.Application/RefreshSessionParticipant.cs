using Shuttle.Access.Query;
using Shuttle.Contract;
using Shuttle.Mediator;

namespace Shuttle.Access.Application;

public class RefreshSessionParticipant(ISessionCache sessionCache, ISessionQuery sessionQuery, IIdentityQuery identityQuery)
    : IParticipant<RefreshSession>
{
    public async Task HandleAsync(RefreshSession message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNull(sessionCache);
        Guard.AgainstNull(sessionQuery);
        Guard.AgainstNull(identityQuery);

        var session = (await sessionQuery.SearchAsync(new Session.Specification().AddId(message.Id), cancellationToken)).FirstOrDefault();

        if (session == null)
        {
            return;
        }

        session.Permissions = (await identityQuery.PermissionsAsync(session.IdentityId, session.TenantId, cancellationToken))
            .Select(permission => new Query.Permission
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Status = permission.Status
            })
            .ToList();

        await sessionQuery.SaveAsync(session, cancellationToken);

        sessionCache.Flush(session.IdentityId);
    }
}