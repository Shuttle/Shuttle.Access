using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Messages.v1
{
    public class IdentityRegistrationRequested
    {
        public IdentityRegistrationRequested(Guid? sessionToken)
        {
            SessionToken = sessionToken;
        }

        public Guid? SessionToken { get; }
        public bool IsAllowed { get; private set; }

        public string RegisteredBy { get; private set; }

        public void Allowed(string registeredBy)
        {
            Guard.AgainstNullOrEmptyString(registeredBy, nameof(registeredBy));

            IsAllowed = true;
            RegisteredBy = registeredBy;
        }
    }
}