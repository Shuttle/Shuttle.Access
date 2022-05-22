using System;

namespace Shuttle.Access.Messages.v1
{
    public class SetIdentityRole
    {
        public Guid IdentityId { get; set; }
        public Guid RoleId { get; set; }
        public bool Active { get; set; }
    }
}