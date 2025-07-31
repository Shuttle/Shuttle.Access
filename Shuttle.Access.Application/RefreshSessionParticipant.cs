using System.Linq;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Application;

public class RefreshSessionParticipant : IParticipant<RefreshSession>
{
    private readonly ISessionService _sessionService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceBus _serviceBus;
    private readonly ISessionQuery _sessionQuery;
    private readonly ISessionRepository _sessionRepository;

    public RefreshSessionParticipant(IServiceBus serviceBus, ISessionService sessionService, IAuthorizationService authorizationService, ISessionRepository sessionRepository, ISessionQuery sessionQuery)
    {
        _serviceBus = Guard.AgainstNull(serviceBus);
        _sessionService = Guard.AgainstNull(sessionService);
        _authorizationService = Guard.AgainstNull(authorizationService);
        _sessionRepository = Guard.AgainstNull(sessionRepository);
        _sessionQuery = Guard.AgainstNull(sessionQuery);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RefreshSession> context)
    {
        Guard.AgainstNull(context);

        var specification = new DataAccess.Session.Specification().WithIdentityId(context.Message.IdentityId);

        var identityName = (await _sessionQuery.SearchAsync(specification, context.CancellationToken)).FirstOrDefault()?.IdentityName;

        if (string.IsNullOrWhiteSpace(identityName))
        {
            return;
        }

        var session = await _sessionRepository.FindAsync(identityName, context.CancellationToken);

        if (session == null)
        {
            return;
        }

        session.ClearPermissions();

        foreach (var permission in await _authorizationService.GetPermissionsAsync(session.IdentityName))
        {
            session.AddPermission(new(permission.Id, permission.Name));
        }

        await _sessionRepository.SaveAsync(session);

        await _sessionService.FlushAsync(context.Message.IdentityId);

        await _serviceBus.PublishAsync(new SessionRefreshed
        {
            IdentityId = context.Message.IdentityId,
            IdentityName = identityName
        });
    }
}