using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Access.DataAccess
{
    public class SessionSpecification
    {
        private readonly List<string> _permissions = new();

        public IEnumerable<string> Permissions => _permissions.AsReadOnly();

        public SessionSpecification AddPermission(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, nameof(permission));

            if (!_permissions.Contains(permission))
            {
                _permissions.Add(permission);
            }

            return this;
        }

        public SessionSpecification AddPermissions(IEnumerable<string> permissions)
        {
            Guard.AgainstNull(permissions, nameof(permissions));

            foreach (var permission in permissions)
            {
                AddPermission(permission);
            }

            return this;
        }
    }
}