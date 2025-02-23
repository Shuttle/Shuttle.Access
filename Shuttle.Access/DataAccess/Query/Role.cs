using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess;

public class Role
{
    public class Specification
    {
        private readonly List<string> _names = new();
        private readonly List<Guid> _permissionIds = new();
        private readonly List<Guid> _rolesIds = new();

        public string NameMatch { get; private set; } = string.Empty;
        public IEnumerable<string> Names => _names.AsReadOnly();
        public IEnumerable<Guid> PermissionIds => _permissionIds.AsReadOnly();
        public bool PermissionsIncluded { get; private set; }
        public IEnumerable<Guid> RoleIds => _rolesIds.AsReadOnly();
        public DateTimeOffset? StartDateRegistered { get; private set; }
        public int MaximumRows { get; private set; }

        public Specification AddName(string name)
        {
            if (!_names.Contains(name))
            {
                _names.Add(name);
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

        public Specification AddPermissionIds(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
            {
                AddPermissionId(id);
            }

            return this;
        }

        public Specification AddRoleId(Guid id)
        {
            if (!_rolesIds.Contains(id))
            {
                _rolesIds.Add(id);
            }

            return this;
        }

        public Specification IncludePermissions()
        {
            PermissionsIncluded = true;

            return this;
        }

        public Specification WithNameMatch(string nameMatch)
        {
            NameMatch = nameMatch;

            return this;
        }

        public Specification WithStartDateRegistered(DateTimeOffset date)
        {
            StartDateRegistered = date;

            return this;
        }

        public Specification WithMaximumRows(int maximumRows)
        {
            MaximumRows = maximumRows;

            return this;
        }
    }
}