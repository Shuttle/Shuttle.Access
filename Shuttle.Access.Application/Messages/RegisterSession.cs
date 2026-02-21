using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Application;

public class RegisterSession(string identityName)
{
    private Guid? _authenticationToken;
    private string _password = string.Empty;
    private List<Messages.v1.Tenant> _tenants = [];

    public bool HasSession => Session != null && SessionToken.HasValue;

    public SqlServer.Models.Identity? Identity { get; set; }

    public string IdentityName { get; } = Guard.AgainstEmpty(identityName);

    public SessionRegistrationType RegistrationType { get; private set; } = SessionRegistrationType.None;
    public SessionRegistrationResult Result { get; private set; } = SessionRegistrationResult.Forbidden;
    public Session? Session { get; private set; }
    public Guid? SessionToken { get; private set; }
    public string SessionTokenExchangeUrl { get; private set; } = string.Empty;

    public Guid? TenantId { get; set; }

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

    public SessionResponse GetSessionResponse(bool registrationRequested)
    {
        if (Identity == null || Session == null)
        {
            return new()
            {
                Result = Result.ToString()
            };
        }

        return new()
        {
            Token = SessionToken,
            Result = Result.ToString(),
            RegistrationRequested = registrationRequested,
            IdentityId = Identity.Id,
            IdentityName = IdentityName,
            SessionTokenExchangeUrl = SessionTokenExchangeUrl,
            ExpiryDate = Session.ExpiryDate,
            Permissions = Session.Permissions.Select(item => item.Name).ToList(),
            DateRegistered = Session.DateRegistered,
            TenantId = Session.TenantId,
            Tenants = _tenants
        };
    }

    public RegisterSession Registered(Guid sessionToken, Session session)
    {
        SessionToken = Guard.AgainstEmpty(sessionToken);
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

    public RegisterSession WithIdentity(SqlServer.Models.Identity identity)
    {
        Identity = Guard.AgainstNull(identity);

        _tenants = Identity.IdentityTenants
            .Where(item => item.Tenant.Status == 1)
            .Select(item => new Messages.v1.Tenant
            {
                Id = item.TenantId,
                Name = item.Tenant.Name,
                LogoSvg = item.Tenant.LogoSvg,
                LogoUrl = item.Tenant.LogoUrl
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

    public RegisterSession WithSessionTokenExchangeUrl(string sessionTokenExchangeUrl)
    {
        SessionTokenExchangeUrl = Guard.AgainstEmpty(sessionTokenExchangeUrl);
        return this;
    }

    public RegisterSession WithTenantId(Guid tenantId)
    {
        TenantId = Guard.AgainstEmpty(tenantId);
        return this;
    }
}