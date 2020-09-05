using System;

namespace Shuttle.Access.WebApi
{
    public class SetPasswordModel
    {
        public string Token { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}