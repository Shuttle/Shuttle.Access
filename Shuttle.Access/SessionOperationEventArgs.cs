using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class SessionOperationEventArgs : EventArgs
    {
        public string Description { get; }

        public SessionOperationEventArgs(string description)
        {
            Guard.AgainstNullOrEmptyString(description, nameof(description));

            Description = description;
        }
    }
}