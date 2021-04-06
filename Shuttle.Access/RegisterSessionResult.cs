using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class RegisterSessionResult
    {
        private static readonly List<string> EmptyPermissions = new List<string>();

        private RegisterSessionResult(string identityName, Guid token, IEnumerable<string> permissions)
        {
            IdentityName = identityName;
            Permissions = permissions;
            Token = token;
        }

        public Guid Token { get; }
        public string IdentityName { get; }
        public IEnumerable<string> Permissions { get; }
        public bool Ok => !Guid.Empty.Equals(Token);

        public static RegisterSessionResult Success(Session session)
        {
            Guard.AgainstNull(session, nameof(session));

            return new RegisterSessionResult(session.IdentityName, session.Token, session.Permissions);
        }
        
        public static RegisterSessionResult Success(string identityName, Guid token, IEnumerable<string> permissions)
        {
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));

            return new RegisterSessionResult(identityName, token, permissions ?? EmptyPermissions);
        }

        public static RegisterSessionResult Failure()
        {
            return new RegisterSessionResult(string.Empty, Guid.Empty, new List<string>());
        }
    }
}