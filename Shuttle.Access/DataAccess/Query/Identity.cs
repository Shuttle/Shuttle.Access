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
            public bool RolesIncluded { get; private set; }
            public Guid? Id { get; private set; }
            public Guid? RoleId { get; private set; }
            public Guid? PermissionId { get; private set; }
            public DateTime? StartDateRegistered { get; private set; }

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

            public Specification WithStartDateRegistered(DateTime date)
            {
                StartDateRegistered = date;

                return this;
            }

            public Specification WithRoleId(Guid roleId)
            {
                RoleId = roleId;

                return this;
            }

            public Specification WithPermissionId(Guid permissionId)
            {
                PermissionId = permissionId;

                return this;
            }
        }
    }
}