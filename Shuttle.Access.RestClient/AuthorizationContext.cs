using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationContext(string bearer)
{
    public Guid? TenantId { get; private set; }
    public string Bearer { get; } = Guard.AgainstEmpty(bearer);

    public BearerAuthenticationContext WithTenantId(Guid tenantId)
    {
        TenantId = Guard.AgainstEmpty(tenantId);
        return this;
    }
}