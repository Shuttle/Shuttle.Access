using System;
using System.Collections.Generic;

namespace Shuttle.Access.WebApi
{
    public class UserRoleStatusModel
    {
        public Guid UserId { get; set; }
        public List<Guid> RoleIds { get; set; }
    }
}