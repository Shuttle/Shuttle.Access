using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.DataAccess;

public class DataStoreAccessService : CachedAccessService, IAccessService
{
    private readonly AccessOptions _accessOptions;
    private readonly string _connectionStringName;
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly ISessionRepository _sessionRepository;

    public DataStoreAccessService(IOptions<AccessOptions> accessOptions, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository)
    {
        _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
        _connectionStringName = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value).ConnectionStringName;
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _sessionRepository = Guard.AgainstNull(sessionRepository);
    }

    public async ValueTask<bool> ContainsAsync(Guid token, CancellationToken cancellationToken = default)
    {
        var result = await base.ContainsAsync(token);

        if (!result)
        {
            await CacheAsync(token, cancellationToken);

            return await base.ContainsAsync(token);
        }

        return true;
    }

    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await base.FlushAsync();
    }

    public async ValueTask<bool> HasPermissionAsync(Guid token, string permission, CancellationToken cancellationToken = default)
    {
        if (!await base.ContainsAsync(token))
        {
            await CacheAsync(token, cancellationToken);
        }

        return await base.HasPermissionAsync(token, permission);
    }

    public async Task RemoveAsync(Guid token, CancellationToken cancellationToken = default)
    {
        await base.RemoveAsync(token);
    }

    public async Task<Access.Session?> FindSessionAsync(Guid token, CancellationToken cancellationToken = default)
    {
        await using (_databaseContextFactory.Create(_connectionStringName))
        {
            return await _sessionRepository.FindAsync(token, cancellationToken);
        }
    }

    private async Task CacheAsync(Guid token, CancellationToken cancellationToken)
    {
        if (await base.ContainsAsync(token))
        {
            return;
        }

        await using (_databaseContextFactory.Create(_connectionStringName))
        {
            var session = await _sessionRepository.FindAsync(token, cancellationToken);

            if (session != null)
            {
                await CacheAsync(token, session.Permissions, _accessOptions.SessionDuration);
            }
        }
    }
}