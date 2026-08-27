using Shuttle.Contract;

namespace Shuttle.Access.Application;

public class SetIdentityAttributeValue(Guid identityId, Guid attributeDefinitionId, string value, bool active, Guid auditTenantId, string auditIdentityName)
    : IAuditInformation
{
    public bool Active { get; } = active;
    public Guid AttributeDefinitionId { get; } = Guard.AgainstEmpty(attributeDefinitionId);
    public Guid AuditTenantId { get; } = Guard.AgainstEmpty(auditTenantId);
    public string AuditIdentityName { get; } = Guard.AgainstEmpty(auditIdentityName);
    public Guid IdentityId { get; } = Guard.AgainstEmpty(identityId);
    public string Value { get; } = Guard.AgainstNull(value);
}
