using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ChangePasswordParticipant(IHashingService hashingService, ISessionQuery sessionQuery, IEventStore eventStore)
    : IParticipant<ChangePassword>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
    private readonly ISessionQuery _sessionRepository = Guard.AgainstNull(sessionQuery);

    public async Task HandleAsync(ChangePassword message, CancellationToken cancellationToken = default)
    {
        var id = Guid.Empty;

        if (message.Token.HasValue)
        {
            var session = (await _sessionRepository.SearchAsync(new Query.Session.Specification().WithTokenHash(_hashingService.Sha256(message.Token.Value.ToString("D"))), cancellationToken)).FirstOrDefault();

            if (session == null)
            {
                throw new ApplicationException(Access.Resources.SessionTokenExpiredException);
            }

            id = session.IdentityId;
        }

        if (message.Id.HasValue)
        {
            id = message.Id.Value;
        }

        var user = new Identity();
        var stream = await _eventStore.GetAsync(id, cancellationToken: cancellationToken);

        if (stream.IsEmpty)
        {
            return;
        }

        stream.Apply(user);
        stream.Add(user.SetPassword(_hashingService.Sha256(message.NewPassword)));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken).ConfigureAwait(false);
    }
}