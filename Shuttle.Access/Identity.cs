﻿using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class Identity
    {
        private readonly Guid _id;
        private readonly List<Guid> _roles = new List<Guid>();
        private string _name;
        private byte[] _passwordHash;

        public Identity(Guid id)
        {
            _id = id;
        }

        public Guid? PasswordResetToken { get; private set; }
        public bool HasPasswordResetToken => PasswordResetToken.HasValue;
        public DateTime? DateActivated { get; private set; }
        public bool Activated => DateActivated.HasValue;

        public Registered Register(string name, byte[] passwordHash, string registeredBy, string generatedPassword,
            bool activated)
        {
            return On(new Registered
            {
                Name = name,
                PasswordHash = passwordHash,
                RegisteredBy = registeredBy,
                GeneratedPassword = generatedPassword ?? string.Empty,
                DateRegistered = DateTime.Now,
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

        private PasswordResetTokenRegistered On(PasswordResetTokenRegistered passwordResetTokenRegistered)
        {
            Guard.AgainstNull(passwordResetTokenRegistered, nameof(passwordResetTokenRegistered));

            PasswordResetToken = passwordResetTokenRegistered.Token;

            return passwordResetTokenRegistered;
        }

        public Activated Activate(DateTime dateActivated)
        {
            return On(new Activated
            {
                Id = _id,
                DateActivated = dateActivated
            });
        }

        private Registered On(Registered registered)
        {
            Guard.AgainstNull(registered, nameof(registered));

            _name = registered.Name;
            _passwordHash = registered.PasswordHash;

            return registered;
        }

        private Activated On(Activated activated)
        {
            Guard.AgainstNull(activated, nameof(activated));

            DateActivated = activated.DateActivated;

            return activated;
        }

        private PasswordSet On(PasswordSet passwordSet)
        {
            Guard.AgainstNull(passwordSet, nameof(passwordSet));

            _passwordHash = passwordSet.PasswordHash;
            PasswordResetToken = null;

            return passwordSet;
        }

        public static string Key(string name)
        {
            return $"[identity]:name={name};";
        }

        public bool PasswordMatches(byte[] hash)
        {
            Guard.AgainstNull(hash, nameof(hash));

            return _passwordHash.SequenceEqual(hash);
        }

        public RoleAdded AddRole(Guid roleId)
        {
            return On(new RoleAdded {RoleId = roleId});
        }

        private RoleAdded On(RoleAdded roleAdded)
        {
            Guard.AgainstNull(roleAdded, nameof(roleAdded));

            _roles.Add(roleAdded.RoleId);

            return roleAdded;
        }

        public bool IsInRole(Guid roleId)
        {
            return _roles.Contains(roleId);
        }

        public RoleRemoved RemoveRole(Guid roleId)
        {
            if (!IsInRole(roleId))
            {
                throw new InvalidOperationException(string.Format(Resources.RoleNotFoundException, roleId, _name));
            }

            return On(new RoleRemoved {RoleId = roleId});
        }

        private RoleRemoved On(RoleRemoved roleRemoved)
        {
            Guard.AgainstNull(roleRemoved, nameof(roleRemoved));

            _roles.Remove(roleRemoved.RoleId);

            return roleRemoved;
        }

        private Removed On(Removed removed)
        {
            Guard.AgainstNull(removed, nameof(removed));

            return removed;
        }

        public Removed Remove()
        {
            return On(new Removed
            {
                Id = _id
            });
        }

        public PasswordSet SetPassword(byte[] passwordHash)
        {
            Guard.AgainstNull(passwordHash, nameof(passwordHash));

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
}