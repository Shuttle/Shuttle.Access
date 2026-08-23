using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SetTenantLogoUrl(Guid id, string logoUrl, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string LogoUrl { get; } = logoUrl;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
}
