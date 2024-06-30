using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1
{
    public class Identity
    {
        public class Role
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public Guid Id { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime? DateActivated { get; set; }
        public string RegisteredBy { get; set; }
        public string Name { get; set; }
        public string GeneratedPassword { get; set; }
        public List<Role> Roles { get; set; } = new();
    }
}