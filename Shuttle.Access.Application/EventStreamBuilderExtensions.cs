using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public static class EventStreamBuilderExtensions
{
    extension(EventStreamBuilder eventStreamBuilder)
    {
        public EventStreamBuilder Audit(IAuditInformation auditInformation)
        {
            Guard.AgainstNull(auditInformation);
            return eventStreamBuilder.Audit(auditInformation.TenantId, auditInformation.IdentityName);
        }
        public EventStreamBuilder Audit(Messages.v1.AuditMessage auditMessage)
        {
            Guard.AgainstNull(auditMessage);
            return eventStreamBuilder.Audit(auditMessage.AuditTenantId, auditMessage.AuditIdentityName);
        }

        public EventStreamBuilder Audit(Guid auditTenantId, string auditIdentityName)
        {
            eventStreamBuilder.AddHeader("AuditTenantId", Guard.AgainstEmpty(auditTenantId).ToString("D"));
            eventStreamBuilder.AddHeader("AuditIdentityName", Guard.AgainstEmpty(auditIdentityName));
            return eventStreamBuilder;
        }
    }
}