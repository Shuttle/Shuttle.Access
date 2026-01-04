using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterSession(string identityName)
{
    private Guid? _authenticationToken;
    private string _password = string.Empty;

    public bool HasKnownApplicationOptions => KnownApplicationOptions != null;

    public bool HasSession => Session != null && SessionToken.HasValue;

    public string IdentityName { get; } = Guard.AgainstEmpty(identityName);

    public KnownApplicationOptions? KnownApplicationOptions { get; private set; }
    public SessionRegistrationType RegistrationType { get; private set; } = SessionRegistrationType.None;
    public SessionRegistrationResult Result { get; private set; } = SessionRegistrationResult.Forbidden;
    public Session? Session { get; private set; }
    public Guid? SessionToken { get; private set; }
    public string SessionTokenExchangeUrl { get; private set; } = string.Empty;

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

    public Guid GetAuthenticationToken()
    {
        if (!_authenticationToken.HasValue)
        {
            throw new InvalidOperationException(Resources.RegisterSessionTokenNotSetException);
        }

        return _authenticationToken.Value;
    }

    public string GetPassword()
    {
        if (string.IsNullOrWhiteSpace(_password))
        {
            throw new InvalidOperationException(Resources.RegisterSessionPasswordNotSetException);
        }

        return _password;
    }

    public RegisterSession Registered(Guid sessionToken, Session session)
    {
        SessionToken = sessionToken;
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

    public RegisterSession UseAuthenticationToken(Guid authenticationToken)
    {
        SetRegistrationType(SessionRegistrationType.Token);

        _authenticationToken = authenticationToken;

        return this;
    }

    public RegisterSession UseDelegation(Guid registrationToken)
    {
        SetRegistrationType(SessionRegistrationType.Delegation);

        _authenticationToken = registrationToken;

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

        _password = Guard.AgainstEmpty(password);

        return this;
    }

    public RegisterSession WithKnownApplicationOptions(KnownApplicationOptions knownApplicationOptions)
    {
        KnownApplicationOptions = Guard.AgainstNull(knownApplicationOptions);

        return this;
    }

    public RegisterSession WithSessionTokenExchangeUrl(string sessionTokenExchangeUrl)
    {
        SessionTokenExchangeUrl = Guard.AgainstEmpty(sessionTokenExchangeUrl);

        return this;
    }
}