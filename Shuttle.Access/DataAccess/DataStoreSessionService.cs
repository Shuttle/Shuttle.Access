using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess;

public class DataStoreSessionService : SessionCache, ISessionService
{
    private readonly string _connectionStringName;
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IHashingService _hashingService;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ISessionRepository _sessionRepository;

    public DataStoreSessionService(IOptions<AccessOptions> accessOptions, IHashingService hashingService, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository)
    {
        _hashingService = Guard.AgainstNull(hashingService);
        _connectionStringName = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value).ConnectionStringName;
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _sessionRepository = Guard.AgainstNull(sessionRepository);
    }

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

            await using (_databaseContextFactory.Create(_connectionStringName))
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

            await using (_databaseContextFactory.Create(_connectionStringName))
            {
                return Add(null, await _sessionRepository.FindAsync(identityId, cancellationToken));
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

            await using (_databaseContextFactory.Create(_connectionStringName))
            {
                return Add(null, await _sessionRepository.FindAsync(identityName, cancellationToken));
            }
        }
        finally
        {
            _lock.Release();
        }
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
            Permissions = session.Permissions.ToList()
        });
    }
}