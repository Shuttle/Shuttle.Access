using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class ReviewIdentityRoleRemoval(Guid tenantId, Guid roleId)
{
    public Guid TenantId { get; set; } = Guard.AgainstEmpty(tenantId);
    public Guid RoleId { get; set; } = Guard.AgainstEmpty(roleId);
    public bool IsLastAdministrator { get; private set; }

    public void LastAdministrator()
    {
        IsLastAdministrator = true;
    }
}