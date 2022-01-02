using System;

namespace Shuttle.Access.Messages.v1
{
    public class RolePermissionStatusSet
    {
        public Guid RoleId { get; set; }
        public string Permission { get; set; }
        public bool Active { get; set; }
    }
}