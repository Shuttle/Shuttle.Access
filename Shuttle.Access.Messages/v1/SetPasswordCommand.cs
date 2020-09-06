using System;

namespace Shuttle.Access.Messages.v1
{
    public class SetPasswordCommand
    {
        public Guid UserId { get; set; }
        public byte[] PasswordHash { get; set; }
    }
}