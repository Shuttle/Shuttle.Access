using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess
{
    public class PermissionSpecification
    {
        private readonly List<Guid> _ids = new();
        private readonly List<string> _names = new();
        private readonly List<Guid> _roleIds = new();
        private readonly List<int> _statuses = new();
        public IEnumerable<Guid> Ids => _ids.AsReadOnly();

        public string NameMatch { get; private set; }
        public IEnumerable<string> Names => _names.AsReadOnly();
        public IEnumerable<Guid> RoleIds => _roleIds.AsReadOnly();
        public IEnumerable<int> Statuses => _statuses.AsReadOnly();

        public PermissionSpecification AddId(Guid id)
        {
            if (!_ids.Contains(id))
            {
                _ids.Add(id);
            }

            return this;
        }

        public PermissionSpecification AddIds(IEnumerable<Guid> ids)
        {
            if (ids == null)
            {
                return this;
            }

            foreach (var id in ids)
            {
                AddId(id);
            }

            return this;
        }

        public PermissionSpecification AddName(string name)
        {
            if (!_names.Contains(name))
            {
                _names.Add(name);
            }

            return this;
        }

        public PermissionSpecification AddRoleId(Guid roleId)
        {
            if (!_roleIds.Contains(roleId))
            {
                _roleIds.Add(roleId);
            }

            return this;
        }

        public PermissionSpecification AddStatus(int status)
        {
            if (!_statuses.Contains(status))
            {
                _statuses.Add(status);
            }

            return this;
        }

        public PermissionSpecification WithNameMatch(string nameMatch)
        {
            NameMatch = nameMatch;

            return this;
        }
    }
}