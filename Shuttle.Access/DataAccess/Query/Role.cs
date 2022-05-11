using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess.Query
{
    public class Role
    {
        public string RoleName { get; set; }
        public Guid Id { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();

        public class RolePermission
        {
            public Guid RoleId { get; set; }
            public string Permission { get; set; }
        }

        public class Specification
        {
            public string RoleNameMatch { get; private set; }
            public string RoleName { get; private set; }
            public Guid? RoleId { get; private set; }
            public bool PermissionsIncluded { get; private set; }
            public DateTime? StartDateRegistered { get; private set; }

            public Specification WithRoleId(Guid roleId)
            {
                RoleId = roleId;

                return this;
            }

            public Specification WithRoleName(string roleName)
            {
                RoleName = roleName;

                return this;
            }

            public Specification WithRoleNameMatch(string roleNameMatch)
            {
                RoleNameMatch = roleNameMatch;

                return this;
            }

            public Specification IncludePermissions()
            {
                PermissionsIncluded = true;

                return this;
            }

            public Specification WithStartDateRegistered(DateTime date)
            {
                StartDateRegistered = date;

                return this;
            }
        }
    }
}