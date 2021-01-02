using System;

namespace Shuttle.Access.Messages.v1
{
    public class IdentityActivatedEvent
    {
        public Guid Id { get; set; }
        public DateTime DateActivated { get; set; }
    }
}