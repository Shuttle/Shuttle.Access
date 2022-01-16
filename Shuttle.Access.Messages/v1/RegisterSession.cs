using System;

namespace Shuttle.Access.Messages.v1
{
    public class RegisterSession
    {
        public string IdentityName { get; set; }
        public string Password { get; set; }
        public Guid Token { get; set; }
    }
}