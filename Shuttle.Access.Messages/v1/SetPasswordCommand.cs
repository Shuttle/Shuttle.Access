using System;

namespace Shuttle.Access.Messages.v1
{
    public class SetPasswordCommand
    {
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
    }
}