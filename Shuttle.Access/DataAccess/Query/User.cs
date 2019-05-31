using System;
using System.Collections.Generic;

namespace Shuttle.Access.DataAccess.Query
{
    public class User
    {
        public User()
        {
            Roles = new List<Guid>();
        }

        public List<Guid> Roles { get; set; }

        public Guid Id { get; set; }
        public DateTime DateRegistered { get; set; }
        public string RegisteredBy { get; set; }
        public string Username { get; set; }
    }
}