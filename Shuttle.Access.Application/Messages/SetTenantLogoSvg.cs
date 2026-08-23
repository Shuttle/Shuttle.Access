using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SetTenantLogoSvg(Guid id, string logoSvg, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public Guid Id { get; } = Guard.AgainstEmpty(id);
    public string LogoSvg { get; } = logoSvg;
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
}
