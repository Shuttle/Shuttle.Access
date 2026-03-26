using Microsoft.Extensions.Options;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Application;

public class SessionTenantSelectedParticipant(IOptions<AccessOptions> accessOptions, ISessionQuery sessionQuery, IIdentityQuery identityQuery, ISessionCache sessionCache) : IParticipant<SessionTenantSelected>
{
    public async Task HandleAsync(SessionTenantSelected message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var session = (await Guard.AgainstNull(sessionQuery).SearchAsync(new Session.Specification().AddId(message.SessionId), cancellationToken)).FirstOrDefault();

        if (session == null)
        {
            return;
        }

        session.TenantId = message.TenantId;
        session.ExpiryDate = DateTime.UtcNow.Add(accessOptions.Value.SessionDuration);
        session.Permissions = (await identityQuery.PermissionsAsync(session.IdentityId, session.TenantId, cancellationToken))
            .Select(permission => new Query.Permission
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                Status = permission.Status
            })
            .ToList();

        await sessionQuery.SaveAsync(session, cancellationToken);

        message.WithSession(session);

        sessionCache.Flush(session.IdentityId);
    }
}