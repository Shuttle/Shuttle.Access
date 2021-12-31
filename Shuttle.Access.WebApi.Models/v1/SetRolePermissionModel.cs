using System;

namespace Shuttle.Access.WebApi.Models.v1
{
    public class SetRolePermissionModel
    {
        public Guid RoleId { get; set; }
        public string Permission { get; set; }
        public bool Active { get; set; }
    }
}