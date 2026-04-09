using System.Diagnostics.CodeAnalysis;
using Shuttle.Access.Query;
using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class RegisterSession(string identityName)
{
    private readonly List<Session> _sessionsRemoved = [];
    private readonly List<Query.Tenant> _tenants = [];
    private string _password = string.Empty;
    private Guid? _sessionToken;

    [MemberNotNullWhen(true, nameof(Session), nameof(SessionToken))]
    public bool HasSession => Session != null;

    public string IdentityName { get; } = Guard.AgainstEmpty(identityName);

    public SessionRegistrationType RegistrationType { get; private set; } = SessionRegistrationType.None;
    public SessionRegistrationResult Result { get; private set; } = SessionRegistrationResult.Forbidden;

    public Session? Session { get; private set; }

    public IEnumerable<Session> SessionsRemoved => _sessionsRemoved.AsReadOnly();
    public Guid? SessionToken { get; private set; }
    public bool ShouldRefresh { get; private set; }
    public Guid? TenantId { get; private set; }

    public IEnumerable<Query.Tenant> Tenants => _tenants.AsReadOnly();


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
        return string.IsNullOrWhiteSpace(_password)
            ? throw new InvalidOperationException(Resources.RegisterSessionPasswordNotSetException)
            : _password;
    }

    public Guid GetSessionToken()
    {
        return _sessionToken ?? throw new InvalidOperationException(Resources.RegisterSessionTokenNotSetException);
    }

    public RegisterSession Refresh()
    {
        if (RegistrationType == SessionRegistrationType.Token)
        {
            throw new InvalidOperationException(Resources.SessionRefreshException);
        }

        ShouldRefresh = true;
        return this;
    }

    public RegisterSession Registered(Guid sessionToken, Session session)
    {
        SessionToken = Guard.AgainstEmpty(sessionToken);
        Session = session;

        Result = SessionRegistrationResult.Registered;

        return this;
    }

    public RegisterSession Renewed(Session session)
    {
        Session = session;

        Result = SessionRegistrationResult.Renewed;

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

    public RegisterSession UseDelegation(Guid tenantId, Guid delegatedSessionToken)
    {
        SetRegistrationType(SessionRegistrationType.Delegation);

        TenantId = Guard.AgainstEmpty(tenantId);
        _sessionToken = Guard.AgainstEmpty(delegatedSessionToken);

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

    public RegisterSession UseSessionToken(Guid authenticationToken)
    {
        if (ShouldRefresh)
        {
            throw new InvalidOperationException(Resources.SessionRefreshException);
        }

        SetRegistrationType(SessionRegistrationType.Token);

        _sessionToken = authenticationToken;

        return this;
    }

    public RegisterSession WithIdentity(Query.Identity identity, Guid systemTenantId)
    {
        Guard.AgainstEmpty(systemTenantId);

        return this;
    }

    public RegisterSession WithSessionsRemoved(IEnumerable<Session> sessions)
    {
        _sessionsRemoved.Clear();
        _sessionsRemoved.AddRange(Guard.AgainstEmpty(sessions));
        return this;
    }

    public RegisterSession WithTenantId(Guid tenantId)
    {
        TenantId = Guard.AgainstEmpty(tenantId);
        return this;
    }

    public RegisterSession WithTenants(IEnumerable<Query.Tenant> tenants)
    {
        _tenants.AddRange(tenants);
        return this;
    }
}