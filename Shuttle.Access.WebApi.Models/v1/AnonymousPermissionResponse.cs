using System.Collections.Generic;

namespace Shuttle.Access.WebApi.Models.v1
{
    public class AnonymousPermissionResponse
    {
        public bool IsIdentityRequired { get; set; }
        public List<PermissionModel> Permissions { get; set; }
    }
}