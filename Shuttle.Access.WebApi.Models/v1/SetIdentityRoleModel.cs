using System;

namespace Shuttle.Access.WebApi.Models.v1
{
    public class SetIdentityRoleModel
    {
        public Guid IdentityId { get; set; }
        public Guid RoleId { get; set; }
        public bool Active { get; set; }
    }
}