using System;

namespace Shuttle.Access.Messages.v1
{
    public class RegisterPasswordResetCommand
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}