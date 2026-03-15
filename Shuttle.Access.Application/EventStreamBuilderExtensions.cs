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
            return eventStreamBuilder.Audit(auditInformation.AuditTenantId, auditInformation.AuditIdentityName);
        }

        public EventStreamBuilder Audit(Guid auditTenantId, string auditIdentityName)
        {
            eventStreamBuilder.AddHeader("AuditTenantId", Guard.AgainstEmpty(auditTenantId).ToString("D"));
            eventStreamBuilder.AddHeader("AuditIdentityName", Guard.AgainstEmpty(auditIdentityName));
            return eventStreamBuilder;
        }
    }
}