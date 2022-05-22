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
            public List<Guid> Ids { get; } = new List<Guid>();
            public bool PermissionsIncluded { get; private set; }
            public DateTime? StartDateRegistered { get; private set; }

            public Specification AddId(Guid id)
            {
                if (!Ids.Contains(id))
                {
                    Ids.Add(id);
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
        }
    }
}