using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application
{
    public class SessionIdentityRegistrationRequestedParticipant : IAsyncParticipant<IdentityRegistrationRequested>
    {
        private readonly ISessionRepository _sessionRepository;

        public SessionIdentityRegistrationRequestedParticipant(ISessionRepository sessionRepository)
        {
            Guard.AgainstNull(sessionRepository, nameof(sessionRepository));

            _sessionRepository = sessionRepository;
        }

        public async Task ProcessMessageAsync(IParticipantContext<IdentityRegistrationRequested> context)
        {
            Guard.AgainstNull(context, nameof(context));

            if (!context.Message.SessionToken.HasValue)
            {
                return;
            }

            var session = await _sessionRepository.FindAsync(context.Message.SessionToken.Value, context.CancellationToken);

            if (session != null && session.HasPermission(Permissions.Register.Identity))
            {
                context.Message.Allowed(session.IdentityName, session.HasPermission(Permissions.Activate.Identity));
            }
        }
    }
}