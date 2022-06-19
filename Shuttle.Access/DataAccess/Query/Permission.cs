using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess.Query
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public string StatusName => ((PermissionStatus)Status).ToString();

        public class Specification
        {
            private readonly List<string> _names = new List<string>();
            private readonly List<Guid> _ids = new List<Guid>();
            private readonly List<Guid> _roleIds = new List<Guid>();
            private readonly List<int> _statuses = new List<int>();
            
            public string NameMatch { get; private set; }
            public IEnumerable<string> Names => _names.AsReadOnly();
            public IEnumerable<Guid> Ids => _ids.AsReadOnly();
            public IEnumerable<Guid> RoleIds => _roleIds.AsReadOnly();
            public IEnumerable<int> Statuses => _statuses.AsReadOnly();

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

            public Specification AddId(Guid id)
            {
                if (!_ids.Contains(id))
                {
                    _ids.Add(id);
                }

                return this;
            }

            public Specification AddIds(IEnumerable<Guid> ids)
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

            public Specification AddStatus(int status)
            {
                if (!_statuses.Contains(status))
                {
                    _statuses.Add(status);
                }

                return this;
            }

            public Specification AddRoleId(Guid roleId)
            {
                if (!_roleIds.Contains(roleId))
                {
                    _roleIds.Add(roleId);
                }

                return this;
            }
        }
    }
}