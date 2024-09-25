using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterSession
{
    private string _password;

    private Guid? _token;

    public RegisterSession(string identityName)
    {
        IdentityName = Guard.AgainstNullOrEmptyString(identityName);
    }

    public bool HasSession => Session != null;

    public string IdentityName { get; }
    public SessionRegistrationType RegistrationType { get; private set; } = SessionRegistrationType.None;
    public SessionRegistrationResult Result { get; private set; } = SessionRegistrationResult.None;
    public Session Session { get; private set; }

    public RegisterSession DelegationSessionInvalid()
    {
        Result = SessionRegistrationResult.DelegationSessionInvalid;

        return this;
    }

    public RegisterSession Forbidden()
    {
        Result = SessionRegistrationResult.Forbidden;

        return this;
    }

    public string GetPassword()
    {
        if (string.IsNullOrWhiteSpace(_password))
        {
            throw new InvalidOperationException(Resources.RegisterSessionPasswordNotSetException);
        }

        return _password;
    }

    public Guid GetToken()
    {
        if (!_token.HasValue)
        {
            throw new InvalidOperationException(Resources.RegisterSessionTokenNotSetException);
        }

        return _token.Value;
    }

    public RegisterSession Registered(Session session)
    {
        Session = Guard.AgainstNull(session);

        Result = SessionRegistrationResult.Registered;

        return this;
    }

    private void SetRegistrationType(SessionRegistrationType type)
    {
        if (RegistrationType != SessionRegistrationType.None)
        {
            throw new InvalidOperationException(string.Format(Resources.SessionRegistrationTypeException, RegistrationType));
        }

        RegistrationType = type;
    }

    public RegisterSession UnknownIdentity()
    {
        Result = SessionRegistrationResult.UnknownIdentity;

        return this;
    }

    public RegisterSession UseDelegation(Guid token)
    {
        SetRegistrationType(SessionRegistrationType.Delegation);

        _token = token;

        return this;
    }

    public RegisterSession UseDirect()
    {
        SetRegistrationType(SessionRegistrationType.Direct);

        return this;
    }

    public RegisterSession UsePassword(string password)
    {
        SetRegistrationType(SessionRegistrationType.Password);

        _password = Guard.AgainstNullOrEmptyString(password);

        return this;
    }

    public RegisterSession UseToken(Guid token)
    {
        SetRegistrationType(SessionRegistrationType.Token);

        _token = token;

        return this;
    }
}