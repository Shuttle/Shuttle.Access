namespace Shuttle.Access.Application;

public enum SessionRegistrationResult
{
    Forbidden,
    Registered,
    DelegationSessionInvalid,
    UnknownIdentity,
    TenantSelectionRequired
}