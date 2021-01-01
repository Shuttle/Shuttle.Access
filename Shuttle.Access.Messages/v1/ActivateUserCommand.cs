using System;

namespace Shuttle.Access.Messages.v1
{
    public class ActivateUserCommand
    {
        public Guid UserId { get; set; }
    }
}