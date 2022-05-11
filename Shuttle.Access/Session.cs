using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class Session
    {
        private readonly List<string> _permissions = new List<string>();

        public Session(Guid token, Guid identityId, string identityName, DateTime dateRegistered, DateTime expiryDate)
        {
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

            Token = token;
            IdentityId = identityId;
            IdentityName = identityName;
            DateRegistered = dateRegistered;
            ExpiryDate = expiryDate;
        }

        public DateTime ExpiryDate { get; private set; }
        public Guid Token { get; private set; }
        public Guid IdentityId { get; }
        public string IdentityName { get; }
        public DateTime DateRegistered { get; set; }

        public bool HasExpired => DateTime.UtcNow >= ExpiryDate;

        public IEnumerable<string> Permissions => new ReadOnlyCollection<string>(_permissions);

        public Session AddPermission(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, "permission");

            if (!HasPermission(permission))
            {
                _permissions.Add(permission);
            }

            return this;
        }

        public bool HasPermission(string permission)
        {
            return _permissions.Find(
                candidate => candidate.Equals(permission, StringComparison.InvariantCultureIgnoreCase)) != null || _permissions.Contains("*");
        }

        public void Renew(DateTime expiryDate)
        {
            Token = Guid.NewGuid();
            ExpiryDate = expiryDate;
        }
    }
}