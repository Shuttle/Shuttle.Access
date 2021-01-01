using System;

namespace Shuttle.Access.Messages.v1
{
    public class UserActivatedEvent
    {
        public Guid UserId { get; set; }
        public DateTime DateActivated { get; set; }
    }
}