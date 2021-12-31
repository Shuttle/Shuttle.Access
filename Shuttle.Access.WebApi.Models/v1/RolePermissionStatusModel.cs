using System;
using System.Collections.Generic;

namespace Shuttle.Access.WebApi.Models.v1
{
    public class RolePermissionStatusModel
    {
        public Guid RoleId { get; set; }
        public List<string> Permissions { get; set; }
    }
}