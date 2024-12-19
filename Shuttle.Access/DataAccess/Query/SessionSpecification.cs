using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Access.DataAccess.Query;

public class Session
{
    public class Specification
    {
        private readonly List<string> _permissions = new();

        public IEnumerable<string> Permissions => _permissions.AsReadOnly();

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
    }
}