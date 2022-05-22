using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess.Query
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }

        public class Specification
        {
            public string NameMatch { get; private set; }
            public List<string> Names { get; } = new List<string>();
            public List<Guid> Ids { get; } = new List<Guid>();
            public List<int> Statuses { get; } = new List<int>();

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

            public Specification AddId(Guid id)
            {
                if (!Ids.Contains(id))
                {
                    Ids.Add(id);
                }

                return this;
            }

            public Specification AddStatus(int status)
            {
                if (!Statuses.Contains(status))
                {
                    Statuses.Add(status);
                }

                return this;
            }
        }
    }
}