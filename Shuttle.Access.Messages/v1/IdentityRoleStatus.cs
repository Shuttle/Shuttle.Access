using System;

namespace Shuttle.Access.Messages.v1
{
    public class IdentityRoleStatus
    {
        public Guid RoleId { get; set; }
        public bool Active { get; set; }
    }
}