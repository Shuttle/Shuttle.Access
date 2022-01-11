using System;

namespace Shuttle.Access.Messages.v1
{
    public class ChangePassword
    {
        public Guid Token { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}