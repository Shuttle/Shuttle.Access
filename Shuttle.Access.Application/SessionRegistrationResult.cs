namespace Shuttle.Access.Application;

public enum SessionRequestResult
{
    Forbidden = 0,
    UnknownIdentity = 1,
    Registered = 2,
    Renewed = 3
}