using Shuttle.Access.Query;

namespace Shuttle.Access.AspNetCore;

public static class SessionContextExtensions
{
    extension(ISessionContext sessionContext)
    {
        public bool HasPermission(string requiredPermission)
        {
            return sessionContext.IsAuthorized && sessionContext.Session.HasPermission(sessionContext.TenantId, requiredPermission);
        }
    }
}