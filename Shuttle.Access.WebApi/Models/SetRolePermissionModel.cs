using System;

namespace Shuttle.Access.WebApi
{
    public class SetRolePermissionModel
    {
        public Guid RoleId { get; set; }
        public string Permission { get; set; }
        public bool Active { get; set; }
    }
}