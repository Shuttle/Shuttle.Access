﻿using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access;

public enum PermissionStatus
{
    Active = 1,
    Deactivated = 2,
    Removed = 3
}

public class Permission
{
    public string Name { get; private set; } = string.Empty;
    public PermissionStatus Status { get; private set; }

    public Activated Activate()
    {
        return On(new Activated());
    }

    public Deactivated Deactivate()
    {
        return On(new Deactivated());
    }

    public static string Key(string name)
    {
        return $"[permission]:name={name}";
    }

    private Registered On(Registered registered)
    {
        Guard.AgainstNull(registered);

        Name = registered.Name;
        Status = registered.Status;

        return registered;
    }

    private Deactivated On(Deactivated deactivated)
    {
        Guard.AgainstNull(deactivated);

        Status = PermissionStatus.Deactivated;

        return deactivated;
    }

    private Activated On(Activated activated)
    {
        Guard.AgainstNull(activated);

        Status = PermissionStatus.Active;

        return activated;
    }

    private Removed On(Removed removed)
    {
        Guard.AgainstNull(removed);

        Status = PermissionStatus.Removed;

        return removed;
    }

    private NameSet On(NameSet nameSet)
    {
        Guard.AgainstNull(nameSet);

        Name = nameSet.Name;

        return nameSet;
    }

    public Registered Register(string name, PermissionStatus status)
    {
        return On(new Registered
        {
            Name = name,
            Status = status
        });
    }

    public Removed Remove()
    {
        return On(new Removed());
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
}