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
            public string Name { get; private set; }
            public string RoleName { get; private set; }
            public bool RolesIncluded { get; set; }
            public Guid? Id { get; private set; }

            public Specification WithIdentityId(Guid id)
            {
                Id = id;

                return this;
            }

            public Specification WithRoleName(string roleName)
            {
                RoleName = roleName;

                return this;
            }

            public Specification WithName(string name)
            {
                Name = name;

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