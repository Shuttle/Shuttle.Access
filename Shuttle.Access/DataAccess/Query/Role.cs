using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess.Query
{
    public class Role
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public List<Permission> Permissions { get; set; } = new List<Permission>();

        public class Permission : Query.Permission
        {
            public Guid RoleId { get; set; }
        }

        public class Specification
        {
            public string NameMatch { get; private set; }
            public List<string> Names { get; } = new List<string>();
            public List<Guid> RoleIds { get; } = new List<Guid>();
            public List<Guid> PermissionIds { get; } = new List<Guid>();
            public bool PermissionsIncluded { get; private set; }
            public DateTime? StartDateRegistered { get; private set; }

            public Specification AddRoleId(Guid id)
            {
                if (!RoleIds.Contains(id))
                {
                    RoleIds.Add(id);
                }

                return this;
            }

            public Specification AddPermissionId(Guid id)
            {
                if (!PermissionIds.Contains(id))
                {
                    PermissionIds.Add(id);
                }

                return this;
            }

            public Specification AddName(string name)
            {
                if (!Names.Contains(name))
                {
                    Names.Add(name);
                }

                return this;
            }

            public Specification WithNameMatch(string nameMatch)
            {
                NameMatch = nameMatch;

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

            public Specification AddPermissionIds(IEnumerable<Guid> ids)
            {
                foreach (var id in ids)
                {
                    AddPermissionId(id);
                }

                return this;
            }
        }
    }
}