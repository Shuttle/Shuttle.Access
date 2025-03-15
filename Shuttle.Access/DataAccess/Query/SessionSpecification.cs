using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Access.DataAccess;

public class Session
{
    public class Specification
    {
        private readonly List<string> _permissions = [];
        public IEnumerable<string> Permissions => _permissions.AsReadOnly();
        public int MaximumRows { get; private set; }

        public Specification AddPermission(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, nameof(permission));

            if (!_permissions.Contains(permission))
            {
                _permissions.Add(permission);
            }

            return this;
        }

        public Specification AddPermissions(IEnumerable<string> permissions)
        {
            Guard.AgainstNull(permissions);

            foreach (var permission in permissions)
            {
                AddPermission(permission);
            }

            return this;
        }

        public Specification WithToken(Guid token)
        {
            Token = token;
            MaximumRows = 1;

            return this;
        }

        public Guid? Token { get; private set; }

        public Specification WithIdentityName(string identityName)
        {
            IdentityName = Guard.AgainstNullOrEmptyString(identityName);
            MaximumRows = 1;

            return this;
        }

        public Specification WithIdentityNameMatch(string identityNameMatch)
        {
            IdentityNameMatch = Guard.AgainstNullOrEmptyString(identityNameMatch);

            return this;
        }

        public string? IdentityName { get; private set; }
        public string? IdentityNameMatch { get; private set; }

        public Specification IncludePermissions()
        {
            ShouldIncludePermissions = true;
            return this;
        }

        public bool ShouldIncludePermissions { get; private set; } = false;

        public Specification WithMaximumRows(int maximumRows)
        {
            MaximumRows = maximumRows;

            return this;
        }
    }
}