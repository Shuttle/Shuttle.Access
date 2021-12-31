using System;
using System.Collections.Generic;

namespace Shuttle.Access.WebApi.Models.v1
{
    public class IdentityRoleStatusModel
    {
        public Guid IdentityId { get; set; }
        public List<Guid> RoleIds { get; set; }
    }
}