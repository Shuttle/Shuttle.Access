using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ActivateIdentityParticipant(IIdentityQuery identityQuery, IEventStore eventStore) : IParticipant<ActivateIdentity>
{
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);

    public async Task HandleAsync(ActivateIdentity message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var now = DateTimeOffset.UtcNow;

        var specification = new IdentitySpecification();

        if (message.Id.HasValue)
        {
            specification.AddId(message.Id.Value);
        }
        else
        {
            specification.WithName(message.Name);
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

        await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken).ConfigureAwait(false);
    }
}