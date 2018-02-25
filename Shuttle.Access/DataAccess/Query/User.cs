using System;
using System.Collections.Generic;

namespace Shuttle.Access.Query
{
    public class User
    {
        public User()
        {
            Roles = new List<string>();
        }

        public List<string> Roles { get; set; }

        public Guid Id { get; set; }
        public DateTime DateRegistered { get; set; }
        public string RegisteredBy { get; set; }
        public string Username { get; set; }
    }
}