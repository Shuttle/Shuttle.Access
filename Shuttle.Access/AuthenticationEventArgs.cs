namespace Shuttle.Access;

public class AuthenticationEventArgs : EventArgs
{
    public AuthenticationEventArgs()
    {
    }

    public AuthenticationEventArgs(bool authenticated, string message)
    {
        Authenticated = authenticated;
        Message = message;
    }

    public bool Authenticated { get; }
    public string Message { get; } = string.Empty;
}