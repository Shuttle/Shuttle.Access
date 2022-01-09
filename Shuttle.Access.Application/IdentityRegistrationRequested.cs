using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application
{
    public class SessionIdentityRegistrationRequestedParticipant:IParticipant<IdentityRegistrationRequested>
    {
        public void ProcessMessage(IParticipantContext<IdentityRegistrationRequested> context)
        {
            throw new System.NotImplementedException();
        }
    }
}