using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ChangePasswordParticipant(IHashingService hashingService, ISessionRepository sessionRepository, IEventStore eventStore)
    : IParticipant<ChangePassword>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);

    public async Task HandleAsync(ChangePassword message, CancellationToken cancellationToken = default)
    {
        var request = Guard.AgainstNull(message).ApplyInvariants();

        var id = Guid.Empty;

        if (request.Token.HasValue)
        {
            var session = await _sessionRepository.FindAsync(new SessionSpecification().WithToken(_hashingService.Sha256(request.Token.Value.ToString("D"))), cancellationToken);

            if (session == null)
            {
                throw new ApplicationException(Access.Resources.SessionTokenExpiredException);
            }

            id = session.IdentityId;
        }

        if (request.Id.HasValue)
        {
            id = request.Id.Value;
        }

        var user = new Identity();
        var stream = await _eventStore.GetAsync(id, cancellationToken: cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        stream.Apply(user);
        stream.Add(user.SetPassword(_hashingService.Sha256(request.NewPassword)));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken).ConfigureAwait(false);
    }
}