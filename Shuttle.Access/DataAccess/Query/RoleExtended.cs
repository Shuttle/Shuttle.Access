using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess.Query
{
    public class RoleExtended
    {
        public RoleExtended()
        {
            Permissions = new List<string>();
        }

        public string RoleName { get; set; }
        public List<string> Permissions { get; set; }
        public Guid Id { get; set; }
    }
}