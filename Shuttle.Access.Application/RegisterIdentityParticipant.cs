using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application
{
    public class RegisterIdentityParticipant : IParticipant<ReviewRequest<RegisterIdentity>>
    {
        public void ProcessMessage(IParticipantContext<ReviewRequest<RegisterIdentity>> context)
        {
            Guard.AgainstNull(context, nameof(context));
        }
    }
}