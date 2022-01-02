using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1
{
    public class GetRolePermissions
    {
        public Guid RoleId { get; set; }
        public List<string> Permissions { get; set; }
    }
}