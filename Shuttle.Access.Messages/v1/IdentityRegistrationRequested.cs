using System;

namespace Shuttle.Access.Messages.v1
{
    public class IdentityRegistrationRequested
    {
        public IdentityRegistrationRequested(Guid? sessionToken)
        {
            SessionToken = sessionToken;
        }

        public Guid? SessionToken { get; }
        public bool IsAllowed { get; private set; } = false;

        public void Allowed()
        {
            IsAllowed = true;
        }
    }
}