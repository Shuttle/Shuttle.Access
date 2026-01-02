using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ChangePasswordParticipant(IHashingService hashingService, ISessionRepository sessionRepository, IEventStore eventStore)
    : IParticipant<RequestMessage<ChangePassword>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);

    public async Task ProcessMessageAsync(RequestMessage<ChangePassword> message, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(message).Request;

        try
        {
            request.ApplyInvariants();
        }
        catch (Exception ex)
        {
            message.Failed(ex.Message);
            throw;
        }

        var id = Guid.Empty;

        if (request.Token.HasValue)
        {
            var session = await _sessionRepository.FindAsync(_hashingService.Sha256(request.Token.Value.ToString("D")));

            if (session == null)
            {
                message.Failed(Access.Resources.SessionTokenExpiredException);

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