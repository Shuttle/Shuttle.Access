using Shuttle.Access.Events.Permission.v1;
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
            return $"[permission]:name={name}";
        }

        public Registered Register(string name, PermissionStatus status)
        {
            return On(new Registered
            {
                Name = name,
                Status = status
            });
        }

        private Registered On(Registered registered)
        {
            Guard.AgainstNull(registered, nameof(registered));

            Name = registered.Name;
            Status = registered.Status;

            return registered;
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
    }
}