using System;

namespace Shuttle.Access.Messages.v1
{
    public class ResetPassword
    {
        public string Name { get; set; }
        public Guid PasswordResetToken { get; set; }
        public string Password { get; set; }
    }
}