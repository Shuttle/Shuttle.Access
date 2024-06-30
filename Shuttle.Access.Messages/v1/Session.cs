using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1
{
    public class Session
    {
        public string IdentityName { get; set; }
        public Guid IdentityId { get; set; }
        public Guid Token { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public List<string> Permissions { get; set; }
    }
}