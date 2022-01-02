using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1
{
    public class SessionRegistered
    {
        public string IdentityName { get; set; }
        public Guid Token { get; set; }
        public DateTime TokenExpiryDate { get; set; }
        public List<string> Permissions { get; set; }
    }
}