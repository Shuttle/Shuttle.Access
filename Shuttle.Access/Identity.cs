﻿using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access;

public class Identity
{
    private readonly List<Guid> _roles = new();
    private byte[]? _passwordHash;
    public bool Activated => DateActivated.HasValue;
    public DateTime? DateActivated { get; private set; }
    public bool HasPasswordResetToken => PasswordResetToken.HasValue;

    public string Name { get; private set; } = string.Empty;
    public Guid? PasswordResetToken { get; private set; }
    public bool Removed { get; private set; }

    public Activated Activate(DateTime dateActivated)
    {
        return On(new Activated
        {
            DateActivated = dateActivated
        });
    }

    public RoleAdded AddRole(Guid roleId)
    {
        if (IsInRole(roleId))
        {
            throw new ApplicationException(string.Format(Resources.DuplicateIdentityRoleException, Name, roleId));
        }

        return On(new RoleAdded { RoleId = roleId });
    }

    public bool IsInRole(Guid roleId)
    {
        return _roles.Contains(roleId);
    }

    public static string Key(string name)
    {
        return $"[identity]:name={name}";
    }

    private PasswordResetTokenRegistered On(PasswordResetTokenRegistered passwordResetTokenRegistered)
    {
        Guard.AgainstNull(passwordResetTokenRegistered);

        PasswordResetToken = passwordResetTokenRegistered.Token;

        return passwordResetTokenRegistered;
    }

    private Registered On(Registered registered)
    {
        Guard.AgainstNull(registered);

        Name = registered.Name;
        _passwordHash = registered.PasswordHash;

        Removed = false;

        return registered;
    }

    private Activated On(Activated activated)
    {
        Guard.AgainstNull(activated);

        DateActivated = activated.DateActivated;

        return activated;
    }

    private PasswordSet On(PasswordSet passwordSet)
    {
        Guard.AgainstNull(passwordSet);

        _passwordHash = passwordSet.PasswordHash;
        PasswordResetToken = null;

        return passwordSet;
    }

    private NameSet On(NameSet nameSet)
    {
        Guard.AgainstNull(nameSet);

        Name = nameSet.Name;

        return nameSet;
    }

    private RoleAdded On(RoleAdded roleAdded)
    {
        Guard.AgainstNull(roleAdded);

        _roles.Add(roleAdded.RoleId);

        return roleAdded;
    }

    private RoleRemoved On(RoleRemoved roleRemoved)
    {
        Guard.AgainstNull(roleRemoved);

        _roles.Remove(roleRemoved.RoleId);

        return roleRemoved;
    }

    private Removed On(Removed removed)
    {
        Guard.AgainstNull(removed);

        Removed = true;

        return removed;
    }

    public bool PasswordMatches(byte[] hash)
    {
        Guard.AgainstNull(hash);

        return _passwordHash != null && _passwordHash.SequenceEqual(hash);
    }

    public Registered Register(string name, byte[] passwordHash, string registeredBy, string generatedPassword, bool activated)
    {
        return On(new Registered
        {
            Name = name,
            PasswordHash = passwordHash,
            RegisteredBy = registeredBy,
            GeneratedPassword = generatedPassword,
            DateRegistered = DateTime.UtcNow,
            Activated = activated
        });
    }

    public PasswordResetTokenRegistered RegisterPasswordResetToken()
    {
        if (HasPasswordResetToken)
        {
            throw new DomainException(Resources.RegisterPasswordResetTokenException);
        }

        return On(new PasswordResetTokenRegistered
        {
            Token = Guid.NewGuid()
        });
    }

    public Removed Remove()
    {
        return On(new Removed());
    }

    public RoleRemoved RemoveRole(Guid roleId)
    {
        if (!IsInRole(roleId))
        {
            throw new InvalidOperationException(string.Format(Resources.RoleNotFoundException, roleId, Name));
        }

        return On(new RoleRemoved { RoleId = roleId });
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

    public PasswordSet SetPassword(byte[] passwordHash)
    {
        Guard.AgainstNull(passwordHash);

        if (passwordHash.Length == 0)
        {
            throw new ArgumentException(Resources.PasswordHashException);
        }

        return On(new PasswordSet
        {
            PasswordHash = passwordHash
        });
    }
}