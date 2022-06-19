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
            private readonly List<string> _names = new List<string>();
            private readonly List<Guid> _rolesIds = new List<Guid>();
            private readonly List<Guid> _permissionIds = new List<Guid>();

            public string NameMatch { get; private set; }
            public IEnumerable<string> Names => _names.AsReadOnly();
            public IEnumerable<Guid> RoleIds => _rolesIds.AsReadOnly();
            public IEnumerable<Guid> PermissionIds => _permissionIds.AsReadOnly();
            public bool PermissionsIncluded { get; private set; }
            public DateTime? StartDateRegistered { get; private set; }

            public Specification AddRoleId(Guid id)
            {
                if (!_rolesIds.Contains(id))
                {
                    _rolesIds.Add(id);
                }

                return this;
            }

            public Specification AddPermissionId(Guid id)
            {
                if (!_permissionIds.Contains(id))
                {
                    _permissionIds.Add(id);
                }

                return this;
            }

            public Specification AddName(string name)
            {
                if (!_names.Contains(name))
                {
                    _names.Add(name);
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