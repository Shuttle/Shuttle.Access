using System;

namespace Shuttle.Access.Messages.v1
{
    public class SetPasswordCommand
    {
        public Guid Id { get; set; }
        public byte[] PasswordHash { get; set; }
    }
}