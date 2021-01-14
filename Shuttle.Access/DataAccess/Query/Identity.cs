using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess.Query
{
    public class Identity
    {
        public class Role
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public Guid Id { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime? DateActivated { get; set; }
        public string RegisteredBy { get; set; }
        public string Name { get; set; }
        public string GeneratedPassword { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();

        public class Specification
        {
            public string RoleName { get; private set; }
            public bool RolesIncluded { get; set; }
            public Guid? IdentityId { get; private set; }

            public Specification WithIdentityId(Guid userId)
            {
                IdentityId = userId;

                return this;
            }

            public Specification WithRoleName(string roleName)
            {
                RoleName = roleName;

                return this;
            }

            public Specification IncludeRoles()
            {
                RolesIncluded = true;

                return this;
            }
        }
    }
}