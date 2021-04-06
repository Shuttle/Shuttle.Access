using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public enum AuthenticationResultType
    {
        Authenticated = 0,
        PasswordExpired = 1,
        Failure = 2
    }

    public class AuthenticationResult
    {
        private AuthenticationResult(bool authenticated, AuthenticationResultType resultType)
        {
            Authenticated = authenticated;
            ResultType = resultType;
        }

        public bool Authenticated { get; }
        public AuthenticationResultType ResultType { get; }

        public static AuthenticationResult Success()
        {
            return new AuthenticationResult(true, AuthenticationResultType.Authenticated);
        }

        public static AuthenticationResult Success(object authenticationTag)
        {
            return new AuthenticationResult(true, AuthenticationResultType.Authenticated);
        }

        public static AuthenticationResult Failure()
        {
            return new AuthenticationResult(false, AuthenticationResultType.Failure);
        }

        public static AuthenticationResult Failure(AuthenticationResultType resultType)
        {
            Guard.Against<ApplicationException>(resultType == AuthenticationResultType.Authenticated,
                "Cannot specify 'Authenticated' as the AuthenticationResultType when authentication has failed.");

            return new AuthenticationResult(false, resultType);
        }
    }
}