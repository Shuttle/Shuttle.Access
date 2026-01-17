using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Application;

public class RefreshSessionParticipant(IServiceBus serviceBus, ISessionService sessionService, IAuthorizationService authorizationService, ISessionRepository sessionRepository)
    : IParticipant<RefreshSession>
{
    private readonly IAuthorizationService _authorizationService = Guard.AgainstNull(authorizationService);
    private readonly IServiceBus _serviceBus = Guard.AgainstNull(serviceBus);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);
    private readonly ISessionService _sessionService = Guard.AgainstNull(sessionService);

    public async Task ProcessMessageAsync(RefreshSession message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var session = await _sessionRepository.FindAsync(message.Id, cancellationToken);

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

        await _sessionService.FlushAsync(session.IdentityId, cancellationToken);

        await _serviceBus.PublishAsync(new SessionRefreshed
        {
            Id = session.Id,
            TenantId = session.TenantId.Value,
            IdentityId = session.IdentityId,
            IdentityName = session.IdentityName
        }, cancellationToken: cancellationToken);
    }
}