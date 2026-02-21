using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class TenantSelectedParticipant(IOptions<AccessOptions> accessOptions, ISessionRepository sessionRepository, IAuthorizationService authorizationService, ISessionCache sessionCache) : IParticipant<TenantSelected>
{
    public async Task HandleAsync(TenantSelected message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var session = await Guard.AgainstNull(sessionRepository).GetAsync(message.SessionId, cancellationToken);

        session.WithTenantId(message.TenantId);

        foreach (var permission in await Guard.AgainstNull(authorizationService).GetPermissionsAsync(session.IdentityName, message.TenantId, cancellationToken))
        {
            session.AddPermission(new(permission.Id, permission.Name));
        }

        session.Renew(DateTimeOffset.UtcNow.Add(accessOptions.Value.SessionDuration));

        await sessionRepository.SaveAsync(session, cancellationToken);

        message.WithSession(session);

        sessionCache.Flush(session.IdentityId);
    }
}