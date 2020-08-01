using System;

namespace Shuttle.Access.DataAccess.Query
{
    public class User
    {
        public Guid Id { get; set; }
        public DateTime DateRegistered { get; set; }
        public string RegisteredBy { get; set; }
        public string Username { get; set; }

        public class Specification
        {
            public string RoleName { get; private set; }

            public Specification WithRoleName(string roleName)
            {
                RoleName = roleName;

                return this;
            }
        }
    }
}