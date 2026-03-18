using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class TenantSelectedParticipant(IOptions<AccessOptions> accessOptions, ISessionRepository sessionRepository, ISessionQuery sessionQuery, IIdentityQuery identityQuery, ISessionCache sessionCache) : IParticipant<TenantSelected>
{
    public async Task HandleAsync(TenantSelected message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var session = await Guard.AgainstNull(sessionRepository).GetAsync(message.SessionId, cancellationToken);

        session.WithTenantId(message.TenantId);
        session.ClearPermissions();

        foreach (var permission in await identityQuery.PermissionsAsync(session.IdentityId, message.TenantId, cancellationToken))
        {
            session.AddPermission(new(permission.Id, permission.Name, permission.Description, permission.Status));
        }

        session.Renew(DateTimeOffset.UtcNow.Add(accessOptions.Value.SessionDuration));

        await sessionRepository.SaveAsync(session, cancellationToken);

        message.WithSession((await sessionQuery.SearchAsync(new Query.Session.Specification().AddId(session.Id), cancellationToken)).First());

        sessionCache.Flush(session.IdentityId);
    }
}