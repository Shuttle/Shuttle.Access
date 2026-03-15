using System.Diagnostics.CodeAnalysis;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterSession(string identityName)
{
    private Guid? _authenticationToken;
    private string _password = string.Empty;
    private List<Query.Tenant> _tenants = [];

    public IEnumerable<Query.Tenant> Tenants => _tenants.AsReadOnly();

    [MemberNotNullWhen(true, nameof(Session))]
    public bool HasSession => Session != null && SessionToken.HasValue;

    public Query.Identity? Identity { get; set; }

    public string IdentityName { get; } = Guard.AgainstEmpty(identityName);

    public SessionRegistrationType RegistrationType { get; private set; } = SessionRegistrationType.None;
    public SessionRegistrationResult Result { get; private set; } = SessionRegistrationResult.Forbidden;
    public Session? Session { get; private set; }
    public Guid? SessionToken { get; private set; }

    public Guid? TenantId { get; private set; }
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
        return _authenticationToken ?? throw new InvalidOperationException(Resources.RegisterSessionTokenNotSetException);
    }

    public string GetPassword()
    {
        return string.IsNullOrWhiteSpace(_password)
            ? throw new InvalidOperationException(Resources.RegisterSessionPasswordNotSetException)
            : _password;
    }

    public RegisterSession Registered(Guid sessionToken, Session session)
    {
        SessionToken = Guard.AgainstEmpty(sessionToken);
        Session = Guard.AgainstNull(session);

        if (TenantId.HasValue)
        {
            var tenantName = Identity?.Tenants.FirstOrDefault(item => item.Id == TenantId)?.Name;

            if (!string.IsNullOrWhiteSpace(tenantName))
            {
                session.WithTenantId(TenantId.Value);
            }
        }
        else
        {
            if (Identity?.Tenants.Count == 1)
            {
                session.WithTenantId(Identity.Tenants.First().Id);
            }
        }

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

    public RegisterSession UseDelegation(Guid tenantId, Guid registrationToken)
    {
        SetRegistrationType(SessionRegistrationType.Delegation);

        TenantId = Guard.AgainstEmpty(tenantId);
        _authenticationToken = Guard.AgainstEmpty(registrationToken);

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

    public RegisterSession WithIdentity(Query.Identity identity)
    {
        Identity = Guard.AgainstNull(identity);

        _tenants = Identity.Tenants
            .Where(item => item.Status == TenantStatus.Active)
            .Select(item => new Query.Tenant
            {
                Id = item.Id,
                Name = item.Name,
                LogoSvg = item.LogoSvg,
                LogoUrl = item.LogoUrl
            })
            .ToList();

        if (_tenants.Count == 0)
        {
            throw new InvalidOperationException(string.Format(Resources.IdentityHasNoTenantsException, IdentityName));
        }

        if (_tenants.Count == 1)
        {
            TenantId = _tenants[0].Id;
        }

        return this;
    }

    public RegisterSession WithTenantId(Guid tenantId)
    {
        TenantId = Guard.AgainstEmpty(tenantId);
        return this;
    }
}