namespace Shuttle.Access.Application;

public enum SessionRegistrationResult
{
    None,
    Registered,
    Forbidden,
    DelegationSessionInvalid,
    UnknownIdentity
}