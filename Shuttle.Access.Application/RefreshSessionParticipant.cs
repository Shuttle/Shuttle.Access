using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class RefreshSessionParticipant(ISessionCache sessionCache, ISessionRepository sessionRepository, IIdentityQuery identityQuery)
    : IParticipant<RefreshSession>
{
    public async Task HandleAsync(RefreshSession message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNull(sessionCache);
        Guard.AgainstNull(sessionRepository);
        Guard.AgainstNull(identityQuery);

        var session = await sessionRepository.FindAsync(new Query.Session.Specification().AddId(message.Id), cancellationToken);

        if (session == null)
        {
            return;
        }

        session.ClearPermissions();

        foreach (var permission in await identityQuery.PermissionsAsync(session.IdentityId, session.TenantId, cancellationToken))
        {
            session.AddPermission(new(permission.Id, permission.Name, permission.Description, permission.Status));
        }

        await sessionRepository.SaveAsync(session, cancellationToken);

        sessionCache.Flush(session.IdentityId);
    }
}