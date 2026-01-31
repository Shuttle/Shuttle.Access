using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Application;

public class RefreshSessionParticipant(IServiceBus serviceBus, ISessionCache sessionCache, IAuthorizationService authorizationService, ISessionRepository sessionRepository)
    : IParticipant<RefreshSession>
{
    private readonly IAuthorizationService _authorizationService = Guard.AgainstNull(authorizationService);
    private readonly IServiceBus _serviceBus = Guard.AgainstNull(serviceBus);
    private readonly ISessionCache _sessionCache = Guard.AgainstNull(sessionCache);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);

    public async Task ProcessMessageAsync(RefreshSession message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var session = await _sessionRepository.FindAsync(new SessionSpecification().AddId(message.Id), cancellationToken);

        if (session is not { TenantId: not null })
        {
            return;
        }

        session.ClearPermissions();

        foreach (var permission in await _authorizationService.GetPermissionsAsync(session.IdentityName, cancellationToken))
        {
            session.AddPermission(new(permission.Id, permission.Name));
        }

        await _sessionRepository.SaveAsync(session, cancellationToken);

        _sessionCache.Flush(session.IdentityId);

        await _serviceBus.PublishAsync(new SessionRefreshed
        {
            Id = session.Id,
            TenantId = session.TenantId.Value,
            IdentityId = session.IdentityId,
            IdentityName = session.IdentityName
        }, cancellationToken: cancellationToken);
    }
}