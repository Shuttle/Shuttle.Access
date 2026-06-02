using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationContext(string bearer)
{
    public Guid? TenantId { get; private set; }
    public string Bearer { get; } = Guard.AgainstEmpty(bearer);
    public string Application { get; private set; } = "Access";

    public BearerAuthenticationContext WithTenantId(Guid tenantId)
    {
        TenantId = Guard.AgainstEmpty(tenantId);
        return this;
    }

    public BearerAuthenticationContext WithScope(string scope)
    {
        Application = Guard.AgainstEmpty(scope);
        return this;
    }
}