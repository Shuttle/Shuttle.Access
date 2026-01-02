using Shuttle.Access.Data;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Application;

public class RefreshSessionParticipant(IServiceBus serviceBus, ISessionService sessionService, IAuthorizationService authorizationService, ISessionRepository sessionRepository, ISessionQuery sessionQuery)
    : IParticipant<RefreshSession>
{
    private readonly IAuthorizationService _authorizationService = Guard.AgainstNull(authorizationService);
    private readonly IServiceBus _serviceBus = Guard.AgainstNull(serviceBus);
    private readonly ISessionQuery _sessionQuery = Guard.AgainstNull(sessionQuery);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);
    private readonly ISessionService _sessionService = Guard.AgainstNull(sessionService);

    public async Task ProcessMessageAsync(RefreshSession message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var specification = new Data.Models.Session.Specification().WithIdentityId(message.IdentityId);

        var identityName = (await _sessionQuery.SearchAsync(specification, cancellationToken)).FirstOrDefault()?.IdentityName;

        if (string.IsNullOrWhiteSpace(identityName))
        {
            return;
        }

        var session = await _sessionRepository.FindAsync(identityName, cancellationToken);

        if (session == null)
        {
            return;
        }

        session.ClearPermissions();

        foreach (var permission in await _authorizationService.GetPermissionsAsync(session.IdentityName, cancellationToken))
        {
            session.AddPermission(new(permission.Id, permission.Name));
        }

        await _sessionRepository.SaveAsync(session, cancellationToken);

        await _sessionService.FlushAsync(message.IdentityId, cancellationToken);

        await _serviceBus.PublishAsync(new SessionRefreshed
        {
            IdentityId = message.IdentityId,
            IdentityName = identityName
        }, cancellationToken: cancellationToken);
    }
}