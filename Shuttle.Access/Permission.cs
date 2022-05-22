﻿using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public enum PermissionStatus
    {
        Active = 1,
        Deactivated = 2,
        Removed = 3
    }

    public class Permission
    {
        public PermissionStatus Status { get; private set; }

        public string Name { get; private set; }

        public static string Key(string name)
        {
            return $"[permission]:name={name};";
        }

        public Added Add(string name, PermissionStatus status)
        {
            return On(new Added
            {
                Name = name,
                Status = status
            });
        }

        private Added On(Added added)
        {
            Guard.AgainstNull(added, nameof(added));

            Name = added.Name;
            Status = added.Status;

            return added;
        }

        public Deactivated Deactivate()
        {
            return On(new Deactivated());
        }

        private Deactivated On(Deactivated deactivated)
        {
            Guard.AgainstNull(deactivated, nameof(deactivated));

            Status = PermissionStatus.Deactivated;

            return deactivated;
        }

        public Activated Activate()
        {
            return On(new Activated());
        }

        private Activated On(Activated activated)
        {
            Guard.AgainstNull(activated, nameof(activated));

            Status = PermissionStatus.Active;

            return activated;
        }

        public Removed Remove()
        {
            return On(new Removed());
        }

        private Removed On(Removed removed)
        {
            Guard.AgainstNull(removed, nameof(removed));

            Status = PermissionStatus.Removed;

            return removed;
        }
    }
}