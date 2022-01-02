using System;

namespace Shuttle.Access.Messages.v1
{
    public class SetPassword
    {
        public Guid Id { get; set; }
        public byte[] PasswordHash { get; set; }
    }
}