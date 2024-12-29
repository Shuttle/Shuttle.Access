using System;
using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ChangePasswordParticipant : IParticipant<RequestMessage<ChangePassword>>
{
    private readonly IEventStore _eventStore;
    private readonly IHashingService _hashingService;
    private readonly ISessionRepository _sessionRepository;

    public ChangePasswordParticipant(IHashingService hashingService, ISessionRepository sessionRepository, IEventStore eventStore)
    {
        _hashingService = Guard.AgainstNull(hashingService);
        _sessionRepository = Guard.AgainstNull(sessionRepository);
        _eventStore = Guard.AgainstNull(eventStore);
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestMessage<ChangePassword>> context)
    {
        Guard.AgainstNull(context);

        var request = context.Message.Request;

        try
        {
            request.ApplyInvariants();
        }
        catch (Exception ex)
        {
            context.Message.Failed(ex.Message);
            throw;
        }

        var id = Guid.Empty;

        if (request.Token.HasValue)
        {
            var session = await _sessionRepository.FindAsync(request.Token.Value);

            if (session == null)
            {
                context.Message.Failed(Access.Resources.SessionTokenExpiredException);

                return;
            }

            id = session.IdentityId;
        }

        if (request.Id.HasValue)
        {
            id = request.Id.Value;
        }

        var user = new Identity();
        var stream = await _eventStore.GetAsync(id);

        if (stream.IsEmpty)
        {
            return;
        }

        stream.Apply(user);
        stream.Add(user.SetPassword(_hashingService.Sha256(request.NewPassword)));

        await _eventStore.SaveAsync(stream);
    }
}