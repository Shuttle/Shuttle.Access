using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.SqlServer;

public class SessionService(IOptions<AccessOptions> accessOptions, IHashingService hashingService, IAuthorizationService authorizationService, IIdentityQuery identityQuery, ISessionRepository sessionRepository)
    : SessionCache, ISessionService
{
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(accessOptions).Value;
    private readonly IAuthorizationService _authorizationService = Guard.AgainstNull(authorizationService);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            Flush();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<bool> HasPermissionAsync(Guid tenantId, Guid identityId, string permission, CancellationToken cancellationToken = default)
    {
        var session = await FindAsync(tenantId, identityId, cancellationToken);

        return session != null && HasPermission(session.IdentityId, permission);
    }

    public async Task FlushAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            Flush(identityId);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddAsync(Guid? token, Messages.v1.Session session, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (Find(session.IdentityId) != null)
            {
                return;
            }

            Add(token, session);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindAsync(Guid token, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = FindByToken(token);

            if (session != null)
            {
                return session;
            }

            return Add(token, await _sessionRepository.FindAsync(_hashingService.Sha256(token.ToString("D")), cancellationToken));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindAsync(Guid tenantId, Guid identityId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = Find(identityId);

            if (session != null)
            {
                return session;
            }

            var aggregate = await _sessionRepository.FindAsync(tenantId, identityId, cancellationToken);

            if (aggregate == null)
            {
                var identity = (await _identityQuery.SearchAsync(new Models.Identity.Specification().AddId(identityId), cancellationToken)).FirstOrDefault();

                if (identity != null)
                {
                    var now = DateTimeOffset.UtcNow;
                    var token = Guid.NewGuid();

                    aggregate = new(Guid.NewGuid(), _hashingService.Sha256(token.ToString("D")), identityId, identity.Name, now, now.Add(_accessOptions.SessionDuration));

                    await SaveAsync(token, aggregate, cancellationToken);
                }
            }

            return Add(null, aggregate);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindAsync(Guid tenantId, string identityName, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = Find(identityName);

            if (session != null)
            {
                return session;
            }

            var aggregate = await _sessionRepository.FindAsync(tenantId, identityName, cancellationToken);

            if (aggregate == null && await _identityQuery.CountAsync(new Models.Identity.Specification().WithName(identityName), cancellationToken) > 0)
            {
                var now = DateTimeOffset.UtcNow;
                var token = Guid.NewGuid();

                aggregate = new(Guid.NewGuid(), _hashingService.Sha256(token.ToString("D")), await _identityQuery.IdAsync(identityName, cancellationToken), identityName, now, now.Add(_accessOptions.SessionDuration));

                await SaveAsync(token, aggregate, cancellationToken);
            }

            return Add(null, aggregate);
        }
        finally
        {
            _lock.Release();
        }
    }

    private Messages.v1.Session? Add(Guid? token, Session? session)
    {
        if (session == null)
        {
            return null;
        }

        return Add(token, new Messages.v1.Session
        {
            TenantId = session.TenantId,
            IdentityId = session.IdentityId,
            IdentityName = session.IdentityName,
            DateRegistered = session.DateRegistered,
            ExpiryDate = session.ExpiryDate,
            Permissions = session.Permissions.Select(item => item.Name).ToList()
        });
    }

    private async Task SaveAsync(Guid token, Session session, CancellationToken cancellationToken)
    {
        foreach (var permission in await _authorizationService.GetPermissionsAsync(session.IdentityName, cancellationToken))
        {
            session.AddPermission(new(permission.Id, permission.Name));
        }

        session.Renew(DateTimeOffset.UtcNow.Add(_accessOptions.SessionDuration), _hashingService.Sha256(token.ToString("D")));

        await _sessionRepository.SaveAsync(session, cancellationToken);
    }
}