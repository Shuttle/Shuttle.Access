using System;

namespace Shuttle.Access.WebApi
{
    public class ResetPasswordModel
    {
        public string Name { get; set; }
        public Guid PasswordResetToken { get; set; }
        public string Password { get; set; }
    }
}