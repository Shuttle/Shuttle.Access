using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class RegisterSessionResult
    {
        private static readonly List<string> EmptyPermissions = new List<string>();

        private RegisterSessionResult(string identityName, Guid token, DateTime tokenExpiryDate,
            IEnumerable<string> permissions)
        {
            IdentityName = identityName;
            Permissions = permissions;
            Token = token;
            TokenExpiryDate = tokenExpiryDate;
        }

        public Guid Token { get; }
        public DateTime TokenExpiryDate { get; }
        public string IdentityName { get; }
        public IEnumerable<string> Permissions { get; }
        public bool Ok => !Guid.Empty.Equals(Token);

        public bool HasExpired()
        {
            return HasExpired(DateTime.Now);
        }
        
        public bool HasExpired(DateTime date)
        {
            return TokenExpiryDate <= date;
        }

        public static RegisterSessionResult Success(Session session)
        {
            Guard.AgainstNull(session, nameof(session));

            return new RegisterSessionResult(session.IdentityName, session.Token, session.ExpiryDate, session.Permissions);
        }
        
        public static RegisterSessionResult Success(string identityName, Guid token, DateTime tokenExpiryDate,
            IEnumerable<string> permissions)
        {
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

            return new RegisterSessionResult(identityName, token, tokenExpiryDate, permissions ?? EmptyPermissions);
        }

        public static RegisterSessionResult Failure()
        {
            return new RegisterSessionResult(string.Empty, Guid.Empty, DateTime.MinValue, new List<string>());
        }
    }
}