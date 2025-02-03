using System;
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

    public async ValueTask<bool> ContainsAsync(Guid token)
    {
        var result = Contains(token);

        if (!result)
        {
            await CacheAsync(token);

            return Contains(token);
        }

        return true;
    }

    public async ValueTask<bool> HasPermissionAsync(Guid token, string permission)
    {
        if (!Contains(token))
        {
            await CacheAsync(token);
        }

        return HasPermission(token, permission);
    }

    public new void Remove(Guid token)
    {
        base.Remove(token);
    }

    private async Task CacheAsync(Guid token)
    {
        if (Contains(token))
        {
            return;
        }

        await using (_databaseContextFactory.Create(_connectionStringName))
        {
            var session = await _sessionRepository.FindAsync(token);

            if (session != null)
            {
                Cache(token, session.Permissions, _accessOptions.SessionDuration);
            }
        }
    }
}