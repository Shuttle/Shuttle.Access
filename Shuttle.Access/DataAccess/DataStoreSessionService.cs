using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess;

public class DataStoreSessionService(IOptions<AccessOptions> accessOptions, IHashingService hashingService, IDatabaseContextFactory databaseContextFactory, IAuthorizationService authorizationService, IIdentityQuery identityQuery, ISessionRepository sessionRepository)
    : SessionCache, ISessionService
{
    private readonly IAuthorizationService _authorizationService = Guard.AgainstNull(authorizationService);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(accessOptions).Value;
    private readonly IDatabaseContextFactory _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
    private readonly IHashingService _hashingService = Guard.AgainstNull(hashingService);
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

    public async ValueTask<bool> HasPermissionAsync(Guid identityId, string permission, CancellationToken cancellationToken = default)
    {
        var session = await FindAsync(identityId, cancellationToken);

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

    public async Task<Messages.v1.Session?> FindByTokenAsync(Guid token, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = FindByToken(token);

            if (session != null)
            {
                return session;
            }

            await using (_databaseContextFactory.Create(_accessOptions.ConnectionStringName))
            {
                return Add(token, await _sessionRepository.FindAsync(_hashingService.Sha256(token.ToString("D")), cancellationToken));
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = Find(identityId);

            if (session != null)
            {
                return session;
            }

            await using (_databaseContextFactory.Create(_accessOptions.ConnectionStringName))
            {
                var aggregate = await _sessionRepository.FindAsync(identityId, cancellationToken);

                if (aggregate == null)
                {
                    var identity = (await _identityQuery.SearchAsync(new Identity.Specification().WithIdentityId(identityId), cancellationToken)).FirstOrDefault();

                    if (identity != null)
                    {
                        var now = DateTimeOffset.UtcNow;
                        var token = Guid.NewGuid();

                        aggregate = new(_hashingService.Sha256(token.ToString("D")), identityId, identity.Name, now, now.Add(_accessOptions.SessionDuration));

                        await SaveAsync(token, aggregate, cancellationToken);
                    }
                }

                return Add(null, aggregate);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Messages.v1.Session?> FindAsync(string identityName, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var session = Find(identityName);

            if (session != null)
            {
                return session;
            }

            await using (_databaseContextFactory.Create(_accessOptions.ConnectionStringName))
            {
                var aggregate = await _sessionRepository.FindAsync(identityName, cancellationToken);

                if (aggregate == null && await _identityQuery.CountAsync(new Identity.Specification().WithName(identityName), cancellationToken) > 0)
                {
                    var now = DateTimeOffset.UtcNow;
                    var token = Guid.NewGuid();

                    aggregate = new(_hashingService.Sha256(token.ToString("D")), await _identityQuery.IdAsync(identityName, cancellationToken), identityName, now, now.Add(_accessOptions.SessionDuration));

                    await SaveAsync(token, aggregate, cancellationToken);
                }

                return Add(null, aggregate);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task SaveAsync(Guid token, Access.Session session, CancellationToken cancellationToken)
    {
        foreach (var permission in await _authorizationService.GetPermissionsAsync(session.IdentityName, cancellationToken))
        {
            session.AddPermission(new(permission.Id, permission.Name));
        }

        session.Renew(DateTimeOffset.UtcNow.Add(_accessOptions.SessionDuration), _hashingService.Sha256(token.ToString("D")));

        await _sessionRepository.SaveAsync(session, cancellationToken);
    }

    private Messages.v1.Session? Add(Guid? token, Access.Session? session)
    {
        if (session == null)
        {
            return null;
        }

        return Add(token, new Messages.v1.Session
        {
            IdentityId = session.IdentityId,
            IdentityName = session.IdentityName,
            DateRegistered = session.DateRegistered,
            ExpiryDate = session.ExpiryDate,
            Permissions = session.Permissions.Select(item => item.Name).ToList()
        });
    }
}