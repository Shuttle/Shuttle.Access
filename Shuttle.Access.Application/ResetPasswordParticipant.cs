using System.Linq;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class ResetPasswordParticipant : IAsyncParticipant<RequestMessage<ResetPassword>>
    {
        private readonly IEventStore _eventStore;
        private readonly IHashingService _hashingService;
        private readonly IIdentityQuery _identityQuery;

        public ResetPasswordParticipant(IHashingService hashingService, IEventStore eventStore, IIdentityQuery identityQuery)
        {
            Guard.AgainstNull(hashingService, nameof(hashingService));
            Guard.AgainstNull(eventStore, nameof(eventStore));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));

            _eventStore = eventStore;
            _hashingService = hashingService;
            _identityQuery = identityQuery;
        }

        public async Task ProcessMessageAsync(IParticipantContext<RequestMessage<ResetPassword>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var queryIdentity = (await _identityQuery.SearchAsync(new IdentitySpecification().WithName(context.Message.Request.Name))).SingleOrDefault();

            if (queryIdentity == null)
            {
                context.Message.Failed(Access.Resources.InvalidCredentialsException);

                return;
            }

            var identity = new Identity();
            var stream = await _eventStore.GetAsync(queryIdentity.Id);

            stream.Apply(identity);

            if (!identity.HasPasswordResetToken || identity.PasswordResetToken != context.Message.Request.PasswordResetToken)
            {
                context.Message.Failed(Access.Resources.InvalidCredentialsException);

                return;
            }

            stream.AddEvent(identity.SetPassword(_hashingService.Sha256(context.Message.Request.Password)));

            await _eventStore.SaveAsync(stream);
        }
    }
}