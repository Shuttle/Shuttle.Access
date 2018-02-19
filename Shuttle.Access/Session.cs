using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class Session
    {
        private readonly List<string> _permissions = new List<string>();

        public Session(Guid token, string username, DateTime dateRegistered)
        {
            Token = token;
            Username = username;
            DateRegistered = dateRegistered;
        }

        public Guid Token { get; private set; }
        public string Username { get; }
        public DateTime DateRegistered { get; set; }

        public IEnumerable<string> Permissions => new ReadOnlyCollection<string>(_permissions);

        public void AddPermission(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, "permission");

            if (_permissions.Find(
                    candidate => candidate.Equals(permission, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                return;
            }

            _permissions.Add(permission);
        }

        public bool HasPermission(string permission)
        {
            return _permissions.Contains(permission) || _permissions.Contains("*");
        }

        public void Renew()
        {
            Token = Guid.NewGuid();
        }
    }
}