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
            Guard.AgainstEmpty(permission, nameof(permission));

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

        public Specification WithToken(byte[] token)
        {
            Token = Guard.AgainstNull(token);
            MaximumRows = 1;

            return this;
        }

        public byte[]? Token { get; private set; }

        public Specification WithIdentityName(string identityName)
        {
            IdentityName = Guard.AgainstEmpty(identityName);
            MaximumRows = 1;

            return this;
        }

        public Specification WithIdentityNameMatch(string identityNameMatch)
        {
            IdentityNameMatch = Guard.AgainstEmpty(identityNameMatch);

            return this;
        }

        public Guid? IdentityId { get; private set; }
        public string? IdentityName { get; private set; }
        public string? IdentityNameMatch { get; private set; }

        public Specification IncludePermissions()
        {
            ShouldIncludePermissions = true;
            return this;
        }

        public bool ShouldIncludePermissions { get; private set; }

        public Specification WithMaximumRows(int maximumRows)
        {
            MaximumRows = maximumRows;

            return this;
        }

        public Specification WithIdentityId(Guid identityId)
        {
            IdentityId = identityId;
            MaximumRows = 1;
            return this;
        }
    }
}