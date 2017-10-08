using System;
using System.Collections.Generic;

namespace Shuttle.Access.WebApi
{
    public class RolePermissionStatusModel
    {
        public Guid RoleId { get; set; }
        public List<string> Permissions { get; set; }
    }
}