using System;

namespace Shuttle.Access.Messages.v1
{
    public class PasswordSetEvent
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
    }
}