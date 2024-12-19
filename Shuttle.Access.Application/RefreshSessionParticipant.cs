using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Application;

public class RefreshSessionParticipant : IParticipant<RefreshSession>
{
    private readonly IAccessService _accessService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceBus _serviceBus;
    private readonly ISessionRepository _sessionRepository;

    public RefreshSessionParticipant(IServiceBus serviceBus, IAccessService accessService, IAuthorizationService authorizationService, ISessionRepository sessionRepository)
    {
        _serviceBus = Guard.AgainstNull(serviceBus);
        _accessService = Guard.AgainstNull(accessService);
        _authorizationService = Guard.AgainstNull(authorizationService);
        _sessionRepository = Guard.AgainstNull(sessionRepository);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RefreshSession> context)
    {
        Guard.AgainstNull(context);

        var session = context.Message.Token.HasValue
            ? await _sessionRepository.FindAsync(context.Message.Token.Value)
            : await _sessionRepository.FindAsync(context.Message.IdentityName);

        if (session == null)
        {
            return;
        }

        session.ClearPermissions();

        foreach (var permission in await _authorizationService.GetPermissionsAsync(session.IdentityName))
        {
            session.AddPermission(permission);
        }

        await _sessionRepository.SaveAsync(session);

        _accessService.Flush(session.Token);

        await _serviceBus.PublishAsync(new SessionRefreshed
        {
            Token = session.Token
        });
    }
}