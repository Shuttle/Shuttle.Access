using System;
using System.Collections.Generic;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class Role
    {
        private readonly List<Guid> _permissionIds = new List<Guid>();
        public string Name { get; private set; }

        public Registered Register(string name)
        {
            return On(new Registered
            {
                Name = name
            });
        }

        private Registered On(Registered registered)
        {
            Guard.AgainstNull(registered, nameof(registered));

            Name = registered.Name;

            return registered;
        }

        public NameSet SetName(string name)
        {
            if (name.Equals(Name))
            {
                throw new DomainException(string.Format(Resources.PropertyUnchangedException, "Name", Name));
            }

            return On(new NameSet
            {
                Name = name
            });
        }

        private NameSet On(NameSet nameSet)
        {
            Guard.AgainstNull(nameSet, nameof(nameSet));

            Name = nameSet.Name;

            return nameSet;
        }

        public static string Key(string name)
        {
            return $"[role]:name={name};";
        }

        public PermissionAdded AddPermission(Guid permissionId)
        {
            if (HasPermission(permissionId))
            {
                throw new InvalidOperationException(string.Format(Resources.DuplicatePermissionException, permissionId,
                    Name));
            }

            return On(new PermissionAdded {PermissionId = permissionId});
        }

        private PermissionAdded On(PermissionAdded permissionAdded)
        {
            Guard.AgainstNull(permissionAdded, nameof(permissionAdded));

            _permissionIds.Add(permissionAdded.PermissionId);

            return permissionAdded;
        }

        public bool HasPermission(Guid permissionId)
        {
            return _permissionIds.Contains(permissionId);
        }

        public PermissionRemoved RemovePermission(Guid permissionId)
        {
            if (!HasPermission(permissionId))
            {
                throw new InvalidOperationException(string.Format(Resources.PermissionNotFoundException, permissionId,
                    Name));
            }

            return On(new PermissionRemoved {PermissionId = permissionId});
        }

        private PermissionRemoved On(PermissionRemoved permissionRemoved)
        {
            Guard.AgainstNull(permissionRemoved, nameof(permissionRemoved));

            _permissionIds.Remove(permissionRemoved.PermissionId);

            return permissionRemoved;
        }

        public Removed Remove()
        {
            return On(new Removed());
        }

        private Removed On(Removed removed)
        {
            Guard.AgainstNull(removed, nameof(removed));

            return removed;
        }
    }
}