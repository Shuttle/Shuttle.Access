using System;

namespace Shuttle.Access.WebApi
{
    public class SetUserRoleModel
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; }
        public bool Active { get; set; }
    }
}