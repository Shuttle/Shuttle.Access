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

    public SessionRepository(IDatabaseContextService databaseContextService, IDataRepository<Session> dataRepository,
        ISessionQueryFactory queryFactory)
    {
        Guard.AgainstNull(databaseContextService);
        Guard.AgainstNull(dataRepository);
        Guard.AgainstNull(queryFactory);

        _databaseContextService = databaseContextService;
        _dataRepository = dataRepository;
        _queryFactory = queryFactory;
    }

    public async Task SaveAsync(Session session, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(session);

        await _databaseContextService.Active.ExecuteAsync(_queryFactory.Save(session), cancellationToken);

        foreach (var permission in session.Permissions)
        {
            await _databaseContextService.Active.ExecuteAsync(_queryFactory.AddPermission(session.Token, permission), cancellationToken);
        }
    }

    public async Task<Session?> FindAsync(Guid token, CancellationToken cancellationToken = default)
    {
        var result = await _dataRepository.FetchItemAsync(_queryFactory.Find(token), cancellationToken);

        if (result == null)
        {
            return null;
        }

        foreach (var row in await _databaseContextService.Active.GetRowsAsync(_queryFactory.GetPermissions(token), cancellationToken))
        {
            result.AddPermission(Columns.PermissionName.Value(row)!);
        }

        return result;
    }

    public async ValueTask<bool> RemoveAsync(Guid token, CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.ExecuteAsync(_queryFactory.Remove(token), cancellationToken) != 0;
    }
}