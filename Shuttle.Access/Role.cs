using System;
using System.Collections.Generic;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class Role
    {
        private readonly Guid _id;
        private readonly List<string> _permissions = new List<string>();
        public string Name { get; private set; }

        public Role(Guid id)
        {
            _id = id;
        }

        public Added Add(string name)
        {
            return On(new Added
            {
                Name = name
            });
        }

        private Added On(Added added)
        {
            Guard.AgainstNull(added, nameof(added));

            Name = added.Name;

            return added;
        }

        public static string Key(string name)
        {
            return string.Format("[role]:name={0};", name);
        }

        public PermissionAdded AddPermission(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, "permission");

            if (HasPermission(permission))
            {
                throw new InvalidOperationException(string.Format(Resources.DuplicatePermissionException, permission,
                    Name));
            }

            return On(new PermissionAdded {Permission = permission});
        }

        private PermissionAdded On(PermissionAdded permissionAdded)
        {
            Guard.AgainstNull(permissionAdded, nameof(permissionAdded));

            _permissions.Add(permissionAdded.Permission);

            return permissionAdded;
        }

        public bool HasPermission(string permission)
        {
            return _permissions.Contains(permission);
        }

        public PermissionRemoved RemovePermission(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, "permission");

            if (!HasPermission(permission))
            {
                throw new InvalidOperationException(string.Format(Resources.PermissionNotFoundException, permission,
                    Name));
            }

            return On(new PermissionRemoved {Permission = permission});
        }

        private PermissionRemoved On(PermissionRemoved permissionRemoved)
        {
            Guard.AgainstNull(permissionRemoved, nameof(permissionRemoved));

            _permissions.Remove(permissionRemoved.Permission);

            return permissionRemoved;
        }

        public Removed Remove()
        {
            return On(new Removed
            {
                Id = _id
            });
        }

        private Removed On(Removed removed)
        {
            Guard.AgainstNull(removed, nameof(removed));

            return removed;
        }
    }
}