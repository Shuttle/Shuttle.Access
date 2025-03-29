using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class SessionRepository : ISessionRepository
{
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IDataRepository<Session> _dataRepository;
    private readonly ISessionQueryFactory _queryFactory;

    public SessionRepository(IDatabaseContextService databaseContextService, IDataRepository<Session> dataRepository, ISessionQueryFactory queryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _dataRepository = Guard.AgainstNull(dataRepository);
        _queryFactory = Guard.AgainstNull(queryFactory);
    }

    public async Task RemoveAllAsync(CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.RemoveAll(), cancellationToken: cancellationToken);
    }

    public async Task SaveAsync(Session session, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(session);

        await _databaseContextService.Active.ExecuteAsync(_queryFactory.Save(session), cancellationToken);
    }

    public async Task<Session?> FindAsync(byte[] token, CancellationToken cancellationToken = default)
    {
        return await FindAsync(new DataAccess.Session.Specification().WithToken(token), cancellationToken);
    }

    public async Task<Session?> FindAsync(string identityName, CancellationToken cancellationToken = default)
    {
        return await FindAsync(new DataAccess.Session.Specification().WithIdentityName(identityName), cancellationToken);
    }

    public async Task<Session?> FindAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        return await FindAsync(new DataAccess.Session.Specification().WithIdentityId(identityId), cancellationToken);
    }

    private async Task<Session?> FindAsync(DataAccess.Session.Specification specification, CancellationToken cancellationToken)
    {
        var result = await _dataRepository.FetchItemAsync(_queryFactory.Search(specification), cancellationToken);

        if (result == null)
        {
            return null;
        }

        foreach (var row in await _databaseContextService.Active.GetRowsAsync(_queryFactory.GetPermissions(result.IdentityId), cancellationToken))
        {
            result.AddPermission(Columns.PermissionName.Value(row)!);
        }

        return result;
    }

    public async ValueTask<bool> RemoveAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.ExecuteAsync(_queryFactory.Remove(identityId), cancellationToken) != 0;
    }
}