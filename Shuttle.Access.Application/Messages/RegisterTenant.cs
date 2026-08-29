using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class RegisterTenant(Guid id, string name, TenantStatus status, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string Name { get; } = Guard.AgainstEmpty(name);
    public TenantStatus Status { get; } = Guard.AgainstUndefinedEnum<TenantStatus>(status);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);

    /// <summary>
    /// When set, an "Access Administrator" role is registered for this tenant and assigned to the identity with
    /// this name, which is also granted access to the tenant.  Leave empty to register the tenant only.
    /// </summary>
    public string AdministratorIdentityName { get; set; } = string.Empty;

    public Guid AccessAdministratorRoleId { get; set; }
}