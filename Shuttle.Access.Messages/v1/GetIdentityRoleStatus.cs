using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1
{
    public class GetIdentityRoleStatus
    {
        public Guid IdentityId { get; set; }
        public List<Guid> RoleIds { get; set; }
    }
}