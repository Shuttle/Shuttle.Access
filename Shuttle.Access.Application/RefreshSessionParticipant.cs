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
    private readonly ISessionQuery _sessionQuery;
    private readonly IAccessService _accessService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceBus _serviceBus;
    private readonly ISessionRepository _sessionRepository;

    public RefreshSessionParticipant(IServiceBus serviceBus, IAccessService accessService, IAuthorizationService authorizationService, ISessionRepository sessionRepository, ISessionQuery sessionQuery)
    {
        _serviceBus = Guard.AgainstNull(serviceBus);
        _accessService = Guard.AgainstNull(accessService);
        _authorizationService = Guard.AgainstNull(authorizationService);
        _sessionRepository = Guard.AgainstNull(sessionRepository);
        _sessionQuery = Guard.AgainstNull(sessionQuery);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RefreshSession> context)
    {
        Guard.AgainstNull(context);

        DataAccess.Session.Specification specification = new();

        if (context.Message.Token.HasValue)
        {
            specification.WithToken(context.Message.Token.Value);
        }
        else
        {
            specification.WithIdentityName(context.Message.IdentityName);
        }

        var token = (await _sessionQuery.SearchAsync(specification, context.CancellationToken)).FirstOrDefault()?.Token;

        if (!token.HasValue)
        {
            return;
        }

        var session = await _sessionRepository.FindAsync(token.Value, context.CancellationToken);

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

        await _accessService.RemoveAsync(session.Token);

        await _serviceBus.PublishAsync(new SessionRefreshed
        {
            Token = session.Token
        });
    }
}