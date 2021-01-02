using System;

namespace Shuttle.Access.WebApi
{
    public class SetIdentityRoleModel
    {
        public Guid IdentityId { get; set; }
        public Guid RoleId { get; set; }
        public bool Active { get; set; }
    }
}