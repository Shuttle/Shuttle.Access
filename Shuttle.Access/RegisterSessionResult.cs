using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class RegisterSessionResult
    {
        private static readonly List<string> EmptyPermissions = new List<string>();

        private RegisterSessionResult(string username, Guid token, IEnumerable<string> permissions)
        {
            Username = username;
            Permissions = permissions;
            Token = token;
        }

        public Guid Token { get; }
        public string Username { get; }
        public IEnumerable<string> Permissions { get; }
        public bool Ok => !Guid.Empty.Equals(Token);

        public static RegisterSessionResult Success(string username, Guid token, IEnumerable<string> permissions)
        {
            Guard.AgainstNullOrEmptyString(username, nameof(username));

            return new RegisterSessionResult(username, token, permissions ?? EmptyPermissions);
        }

        public static RegisterSessionResult Failure()
        {
            return new RegisterSessionResult(string.Empty, Guid.Empty, new List<string>());
        }
    }
}