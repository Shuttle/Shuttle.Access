namespace Shuttle.Access.Application;

public enum SessionRegistrationResult
{
    Forbidden = 0,
    Registered = 1,
    DelegationSessionInvalid = 2,
    UnknownIdentity = 3,
    Renewed = 4
}