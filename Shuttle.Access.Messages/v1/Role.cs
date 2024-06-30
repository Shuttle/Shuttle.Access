using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1
{
    public class Role
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public List<Permission> Permissions { get; set; } = new List<Permission>();

        public class Permission : v1.Permission
        {
            public Guid RoleId { get; set; }
        }
    }
}