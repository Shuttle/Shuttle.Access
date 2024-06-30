using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public class RoleSpecification
    {
        private readonly List<string> _names = new();
        private readonly List<Guid> _permissionIds = new();
        private readonly List<Guid> _rolesIds = new();

        public string NameMatch { get; private set; }
        public IEnumerable<string> Names => _names.AsReadOnly();
        public IEnumerable<Guid> PermissionIds => _permissionIds.AsReadOnly();
        public bool PermissionsIncluded { get; private set; }
        public IEnumerable<Guid> RoleIds => _rolesIds.AsReadOnly();
        public DateTime? StartDateRegistered { get; private set; }

        public RoleSpecification AddName(string name)
        {
            if (!_names.Contains(name))
            {
                _names.Add(name);
            }

            return this;
        }

        public RoleSpecification AddPermissionId(Guid id)
        {
            if (!_permissionIds.Contains(id))
            {
                _permissionIds.Add(id);
            }

            return this;
        }

        public RoleSpecification AddPermissionIds(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                AddPermissionId(id);
            }

            return this;
        }

        public RoleSpecification AddRoleId(Guid id)
        {
            if (!_rolesIds.Contains(id))
            {
                _rolesIds.Add(id);
            }

            return this;
        }

        public RoleSpecification IncludePermissions()
        {
            PermissionsIncluded = true;

            return this;
        }

        public RoleSpecification WithNameMatch(string nameMatch)
        {
            NameMatch = nameMatch;

            return this;
        }

        public RoleSpecification WithStartDateRegistered(DateTime date)
        {
            StartDateRegistered = date;

            return this;
        }
    }
}