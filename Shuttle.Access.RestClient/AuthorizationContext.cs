using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationContext(Guid tenantId, string bearer)
{
    public Guid TenantId { get; } = Guard.AgainstEmpty(tenantId);
    public string Bearer { get; } = Guard.AgainstEmpty(bearer);
}