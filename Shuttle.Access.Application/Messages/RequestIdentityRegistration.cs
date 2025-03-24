using System;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RequestIdentityRegistration
{
    public RegisterIdentity RegisterIdentityMessage { get; }

    public RequestIdentityRegistration(RegisterIdentity registerIdentityMessage)
    {
        RegisterIdentityMessage = Guard.AgainstNull(registerIdentityMessage);
    }

    public bool IsActivationAllowed { get; private set; }
    public bool IsAllowed { get; private set; }

    public string RegisteredBy { get; private set; } = string.Empty;

    public Guid? IdentityId { get; private set; }

    public RequestIdentityRegistration Allowed(string registeredBy, bool activationAllowed)
    {
        Guard.AgainstNullOrEmptyString(registeredBy);

        IsAllowed = true;
        RegisteredBy = registeredBy;
        IsActivationAllowed = activationAllowed;

        return this;
    }

    public RequestIdentityRegistration Allowed(string registeredBy)
    {
        IsAllowed = true;
        RegisteredBy = registeredBy;
        IsActivationAllowed = true;

        return this;
    }

    public RequestIdentityRegistration WithIdentityId(Guid identityId)
    {
        IdentityId = identityId;

        return this;
    }
}