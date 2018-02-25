using System.Collections.Generic;

namespace Shuttle.Access.Query
{
    public class Role
    {
        public Role()
        {
            Permissions = new List<string>();
        }

        public string Name { get; set; }
        public List<string> Permissions { get; set; }
    }
}