using System;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class GetPasswordResetTokenParticipant : IParticipant<RequestResponseMessage<GetPasswordResetToken, Guid>>
    {
        private readonly IEventStore _eventStore;
        private readonly IIdentityQuery _identityQuery;

        public GetPasswordResetTokenParticipant(IIdentityQuery identityQuery, IEventStore eventStore)
        {
            _identityQuery = identityQuery;
            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<RequestResponseMessage<GetPasswordResetToken, Guid>> context)
        {
            var identityName = context.Message.Request.Name;
            var query = _identityQuery.Search(new DataAccess.Query.Identity.Specification().WithName(identityName))
                .SingleOrDefault();

            if (query == null)
            {
                context.Message.Failed(string.Format(Resources.UnknownIdentityException, identityName));

                return;
            }

            var stream = _eventStore.Get(query.Id);
            var identity = new Identity();

            stream.Apply(identity);

            if (identity.Activated)
            {
                if (!identity.HasPasswordResetToken)
                {
                    stream.AddEvent(identity.RegisterPasswordResetToken());

                    _eventStore.Save(stream);
                }

                context.Message.WithResponse(identity.PasswordResetToken.Value);
            }
            else
            {
                context.Message.Failed(string.Format(Resources.IdentityInactiveException, identityName));
            }
        }
    }
}