using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class RegisterTenant(Guid id, string name, TenantStatus status, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string LogoSvg { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string Name { get; } = Guard.AgainstEmpty(name);
    public TenantStatus Status { get; } = Guard.AgainstUndefinedEnum<TenantStatus>(status);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
}