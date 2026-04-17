using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class RegisterPermission(Guid id, string name, string description, PermissionStatus status, Guid auditTenantId, string auditIdentityName)
: IAuditInformation
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string Name { get; } = Guard.AgainstEmpty(name);
    public string Description { get; } = description;
    public PermissionStatus Status { get; } = Guard.AgainstUndefinedEnum<PermissionStatus>(status);
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
}