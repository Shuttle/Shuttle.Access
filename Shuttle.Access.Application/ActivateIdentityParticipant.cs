using System;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class ActivateIdentityParticipant : IParticipant<RequestResponseMessage<ActivateIdentity, IdentityActivated>>
{
    private readonly IEventStore _eventStore;
    private readonly IIdentityQuery _identityQuery;

    public ActivateIdentityParticipant(IIdentityQuery identityQuery, IEventStore eventStore)
    {
        Guard.AgainstNull(identityQuery);
        Guard.AgainstNull(eventStore);

        _identityQuery = identityQuery;
        _eventStore = eventStore;
    }

    public async Task ProcessMessageAsync(IParticipantContext<RequestResponseMessage<ActivateIdentity, IdentityActivated>> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message.Request;
        var now = DateTime.UtcNow;

        var specification = new DataAccess.Query.Identity.Specification();

        if (message.Id.HasValue)
        {
            specification.WithIdentityId(message.Id.Value);
        }
        else
        {
            specification.WithName(message.Name);
        }

        var query = (await _identityQuery.SearchAsync(specification, context.CancellationToken)).FirstOrDefault();

        if (query == null)
        {
            return;
        }

        var id = query.Id;
        var identity = new Identity();
        var stream = await _eventStore.GetAsync(id);

        stream.Apply(identity);
        stream.Add(identity.Activate(now));

        context.Message.WithResponse(new()
        {
            Id = id,
            DateActivated = now,
            SequenceNumber = await _eventStore.SaveAsync(stream)
        });
    }
}