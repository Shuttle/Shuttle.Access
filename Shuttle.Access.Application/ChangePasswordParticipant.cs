using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application
{
    public class ChangePasswordParticipant : IParticipant<RequestMessage<ChangePassword>>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventStore _eventStore;
        private readonly IHashingService _hashingService;
        private readonly ISessionRepository _sessionRepository;

        public ChangePasswordParticipant(IAuthenticationService authenticationService, IHashingService hashingService, ISessionRepository sessionRepository, IEventStore eventStore)
        {
            Guard.AgainstNull(authenticationService, nameof(authenticationService));
            Guard.AgainstNull(hashingService, nameof(hashingService));
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));
            Guard.AgainstNull(eventStore, nameof(eventStore));

            _authenticationService = authenticationService;
            _hashingService = hashingService;
            _sessionRepository = sessionRepository;
            _eventStore = eventStore;
        }

        public void ProcessMessage(IParticipantContext<RequestMessage<ChangePassword>> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var request = context.Message.Request;
            var session = _sessionRepository.Find(request.Token);

            if (session == null)
            {
                context.Message.Failed(Resources.SessionTokenExpiredException);

                return;
            }

            var authenticationResult = _authenticationService.Authenticate(session.IdentityName, request.OldPassword);

            if (!authenticationResult.Authenticated)
            {
                context.Message.Failed(Resources.InvalidCredentialsException);
            }

            var user = new Identity(session.IdentityId);
            var stream = _eventStore.Get(session.IdentityId);

            stream.Apply(user);
            stream.AddEvent(user.SetPassword(_hashingService.Sha256(request.NewPassword)));

            _eventStore.Save(stream);
        }
    }
}