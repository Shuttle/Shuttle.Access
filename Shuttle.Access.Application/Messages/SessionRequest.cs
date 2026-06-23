using System.Diagnostics.CodeAnalysis;
using Shuttle.Access.Query;
using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SessionRequest(string identityName)
{
    private readonly List<Query.Tenant> _tenants = [];
    private string _password = string.Empty;

    [MemberNotNullWhen(true, nameof(Session), nameof(SessionToken))]
    public bool HasSession => Session != null;

    public string IdentityName { get; } = Guard.AgainstEmpty(identityName);
    public string Application { get; private set; } = "Access";

    public SessionRequestType RequestType { get; private set; } = SessionRequestType.None;
    public SessionRequestResult Result { get; private set; } = SessionRequestResult.Forbidden;

    public Session? Session { get; private set; }
    public Guid? SessionToken { get; private set; }

    public IEnumerable<Query.Tenant> Tenants => _tenants.AsReadOnly();
    
    public SessionRequest Forbidden()
    {
        Result = SessionRequestResult.Forbidden;
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
        return SessionToken ?? throw new InvalidOperationException(Resources.RegisterSessionTokenNotSetException);
    }

    public SessionRequest Registered(Guid sessionToken, Session session)
    {
        SessionToken = Guard.AgainstEmpty(sessionToken);
        Session = session;

        Result = SessionRequestResult.Registered;

        return this;
    }

    public SessionRequest Renewed(Session session)
    {
        Session = session;

        Result = SessionRequestResult.Renewed;

        return this;
    }

    private void SetRegistrationType(SessionRequestType type)
    {
        if (RequestType != SessionRequestType.None)
        {
            throw new InvalidOperationException(string.Format(Resources.SessionRegistrationTypeException, RequestType));
        }

        RequestType = type;
    }

    public SessionRequest UnknownIdentity()
    {
        Result = SessionRequestResult.UnknownIdentity;

        return this;
    }

    public SessionRequest UseDirect()
    {
        SetRegistrationType(SessionRequestType.Direct);

        return this;
    }

    public SessionRequest UsePassword(string password)
    {
        SetRegistrationType(SessionRequestType.Password);

        _password = Guard.AgainstEmpty(password);

        return this;
    }

    public SessionRequest UseSessionToken(Guid sessionToken)
    {
        SetRegistrationType(SessionRequestType.Token);

        SessionToken = sessionToken;

        return this;
    }

    public SessionRequest WithIdentity(Query.Identity identity, Guid systemTenantId)
    {
        Guard.AgainstEmpty(systemTenantId);

        return this;
    }

    public SessionRequest WithTenants(IEnumerable<Query.Tenant> tenants)
    {
        _tenants.AddRange(tenants);
        return this;
    }

    public SessionRequest WithApplication(string application)
    {
        Application = Guard.AgainstEmpty(application);
        return this;
    }
}