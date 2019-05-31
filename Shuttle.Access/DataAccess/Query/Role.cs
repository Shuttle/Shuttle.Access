﻿using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess.Query
{
    public class Role
    {
        public class Specification
        {
            public string RoleNameMatch { get; private set; }
            public string RoleName { get; private set; }

            public Specification WithRoleName(string roleName)
            {
                RoleName = roleName;

                return this;
            }

            public Specification WithRoleNameMatch(string roleNameMatch)
            {
                RoleNameMatch = roleNameMatch;

                return this;
            }
        }

        public Role()
        {
            Permissions = new List<string>();
        }

        public string Name { get; set; }
        public List<string> Permissions { get; set; }
        public Guid Id { get; set; }
    }
}