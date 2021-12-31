using System;

namespace Shuttle.Access.WebApi.Models.v1
{
    public class ResetPasswordModel
    {
        public string Name { get; set; }
        public Guid PasswordResetToken { get; set; }
        public string Password { get; set; }
    }
}