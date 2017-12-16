using System;
using System.Collections.Generic;

namespace Shuttle.Access
{
    public class RegisterSessionResult
    {
        private RegisterSessionResult(Guid token, IEnumerable<string> permissions)
        {
            Permissions = permissions;
            Token = token;
        }

        public Guid Token { get; }
        public IEnumerable<string> Permissions { get; }
        public bool Ok => !Guid.Empty.Equals(Token);

        public static RegisterSessionResult Success(Guid token, IEnumerable<string> permissions)
        {
            return new RegisterSessionResult(token, permissions);
        }

        public static RegisterSessionResult Failure()
        {
            return new RegisterSessionResult(Guid.Empty, new List<string>());
        }
    }
}