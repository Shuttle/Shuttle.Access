using System;

namespace Shuttle.Access.WebApi
{
    public class SetUserRoleModel
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public bool Active { get; set; }
    }
}