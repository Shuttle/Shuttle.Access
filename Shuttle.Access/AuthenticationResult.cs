namespace Shuttle.Access;

public class AuthenticationResult(bool authenticated, string? failureReason = null)
{
    public static AuthenticationResult Success = new(true);
    public static AuthenticationResult Failure = new(false);

    public bool Authenticated { get; } = authenticated;
    public string? FailureReason { get; } = failureReason;
}