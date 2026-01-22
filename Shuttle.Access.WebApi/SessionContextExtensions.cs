using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi;

public static class SessionContextExtensions
{
    extension(ISessionContext sessionContext)
    {
        public object Audit(AuditMessage message)
        {
            Guard.AgainstNull(sessionContext).AuthenticationInvariants();

            message.AuditTenantId = sessionContext.Session!.TenantId!.Value;
            message.AuditIdentityName = sessionContext.Session!.IdentityName;

            return message;
        }

        public ISessionContext AuthenticationInvariants()
        {
            if (sessionContext.Session is not { TenantId: not null } ||
                string.IsNullOrWhiteSpace(sessionContext.Session.IdentityName))
            {
                throw new ApplicationException("There is no authenticated session.");
            }

            return sessionContext;
        }

        public IAuditInformation GetAuditInformation()
        {
            Guard.AgainstNull(sessionContext).AuthenticationInvariants();

            return new AuditInformation(sessionContext.Session!.TenantId!.Value, sessionContext.Session!.IdentityName);
        }
    }
}