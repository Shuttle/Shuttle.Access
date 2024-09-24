namespace Shuttle.Access
{
    public class AuthenticationResult
    {
        public static AuthenticationResult Success = new AuthenticationResult(true);
        public static AuthenticationResult Failure = new AuthenticationResult(false);

        public AuthenticationResult(bool authenticated, string failureReason = null)
        {
            Authenticated = authenticated;
            FailureReason = failureReason;
        }

        public bool Authenticated { get; }
        public string FailureReason { get; }
    }
}