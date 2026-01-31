using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ActivateIdentityParticipant(IIdentityQuery identityQuery, IEventStore eventStore) : IParticipant<RequestResponseMessage<ActivateIdentity, IdentityActivated>>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);

    public async Task ProcessMessageAsync(RequestResponseMessage<ActivateIdentity, IdentityActivated> message, CancellationToken cancellationToken = default)
    {
        var request = message.Request;
        var now = DateTimeOffset.UtcNow;

        var specification = new IdentitySpecification();

        if (request.Id.HasValue)
        {
            specification.AddId(request.Id.Value);
        }
        else
        {
            specification.WithName(request.Name);
        }

        var query = (await _identityQuery.SearchAsync(specification, cancellationToken)).FirstOrDefault();

        if (query == null)
        {
            return;
        }

        var id = query.Id;
        var identity = new Identity();
        var stream = await _eventStore.GetAsync(id, cancellationToken: cancellationToken);

        stream.Apply(identity);
        stream.Add(identity.Activate(now));

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message.Request), cancellationToken).ConfigureAwait(false);

        message.WithResponse(new()
        {
            Id = id,
            DateActivated = now
        });
    }
}