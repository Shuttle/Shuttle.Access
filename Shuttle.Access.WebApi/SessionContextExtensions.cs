using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Contract;

namespace Shuttle.Access.WebApi;

public static class SessionContextExtensions
{
    extension(ISessionContext sessionContext)
    {
        public T Audit<T>(T message) where T : AuditMessage
        {
            Guard.AgainstNull(sessionContext).AuthenticationInvariants();

            message.AuditTenantId = sessionContext.TenantId;
            message.AuditIdentityName = sessionContext.Session.IdentityName;

            return message;
        }

        public ISessionContext AuthenticationInvariants()
        {
            return !sessionContext.IsAuthorized ? throw new ApplicationException("There is no authenticated session.") : sessionContext;
        }

        public IAuditInformation GetAuditInformation()
        {
            Guard.AgainstNull(sessionContext).AuthenticationInvariants();

            return new AuditInformation(sessionContext.TenantId, sessionContext.Session.IdentityName);
        }
    }
}