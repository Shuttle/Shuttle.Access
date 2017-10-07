using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Access.Events.User.v1;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access
{
    public class User
    {
        private readonly Guid _id;
        private byte[] _passwordHash;
        private readonly List<string> _roles = new List<string>();
        private string _username;

        public User(Guid id)
        {
            _id = id;
        }

        public Registered Register(string username, byte[] passwordHash, string registeredBy)
        {
            return On(new Registered
            {
                Username = username,
                PasswordHash = passwordHash,
                RegisteredBy = registeredBy,
                DateRegistered = DateTime.Now
            });
        }

        private Registered On(Registered registered)
        {
            Guard.AgainstNull(registered, "registered");

            _username = registered.Username;
            _passwordHash = registered.PasswordHash;

            return registered;
        }

        public static string Key(string username)
        {
            return string.Format("[user]:username={0};", username);
        }

        public bool PasswordMatches(byte[] hash)
        {
            Guard.AgainstNull(hash, "hash");

            return _passwordHash.SequenceEqual(hash);
        }

        public RoleAdded AddRole(string role)
        {
            return On(new RoleAdded {Role = role});
        }

        private RoleAdded On(RoleAdded roleAdded)
        {
            Guard.AgainstNull(roleAdded, "roleAdded");

            _roles.Add(roleAdded.Role);

            return roleAdded;
        }

        public bool IsInRole(string role)
        {
            return _roles.Contains(role);
        }

        public RoleRemoved RemoveRole(string role)
        {
            Guard.AgainstNullOrEmptyString(role, "role");

            if (!IsInRole(role))
            {
                throw new InvalidOperationException(string.Format(AccessResources.RoleNotFoundException, role, _username));
            }

            return On(new RoleRemoved { Role = role });
        }

        private RoleRemoved On(RoleRemoved roleRemoved)
        {
            Guard.AgainstNull(roleRemoved, "roleRemoved");

            _roles.Remove(roleRemoved.Role);

            return roleRemoved;
        }

        private Removed On(Removed removed)
        {
            Guard.AgainstNull(removed, "removed");

            return removed;
        }

        public Removed Remove()
        {
            return On(new Removed
            {
                Id = _id
            });
        }
    }
}