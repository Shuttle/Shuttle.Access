using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class ActivateIdentityParticipant : IParticipant<RequestResponseMessage<ActivateIdentity, IdentityActivated>>
    {
        private readonly IIdentityQuery _identityQuery;
        private readonly IEventStore _eventStore;

        public ActivateIdentityParticipant(IIdentityQuery identityQuery, IEventStore eventStore)
        {
            Guard.AgainstNull(identityQuery, nameof(identityQuery));
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _identityQuery = identityQuery;
            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<ActivateIdentity, IdentityActivated>> context)
        {
            Guard.AgainstNull(context, nameof(context));

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

            Guid id;

            var query = _identityQuery.Search(specification).FirstOrDefault();

            if (query == null)
            {
                return;
            }

            id = query.Id;

            var identity = new Identity();
            var stream = _eventStore.Get(id);

            stream.Apply(identity);
            stream.AddEvent(identity.Activate(now));

            context.Message.WithResponse(new IdentityActivated
            {
                Id = id,
                DateActivated = now,
                SequenceNumber = _eventStore.Save(stream)
            });
        }
    }
}