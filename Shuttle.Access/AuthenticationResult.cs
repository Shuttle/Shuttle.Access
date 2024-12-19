namespace Shuttle.Access;

public class AuthenticationResult
{
    public static AuthenticationResult Success = new(true);
    public static AuthenticationResult Failure = new(false);

    public AuthenticationResult(bool authenticated, string? failureReason = null)
    {
        Authenticated = authenticated;
        FailureReason = failureReason;
    }

    public bool Authenticated { get; }
    public string? FailureReason { get; }
}