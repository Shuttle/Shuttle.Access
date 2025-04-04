﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql;

public class SessionQuery : ISessionQuery
{
    private readonly IDatabaseContextService _databaseContextService;
    private readonly ISessionQueryFactory _queryFactory;
    private readonly IQueryMapper _queryMapper;

    public SessionQuery(IDatabaseContextService databaseContextService, IQueryMapper queryMapper, ISessionQueryFactory queryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _queryMapper = Guard.AgainstNull(queryMapper);
        _queryFactory = Guard.AgainstNull(queryFactory);
    }

    public async ValueTask<int> CountAsync(DataAccess.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Count(specification), cancellationToken);
    }

    public async ValueTask<bool> ContainsAsync(DataAccess.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Contains(specification), cancellationToken) == 1;
    }

    public async Task<IEnumerable<Messages.v1.Session>> SearchAsync(DataAccess.Session.Specification specification, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(specification);

        var sessions = await _queryMapper.MapObjectsAsync<Messages.v1.Session>(_queryFactory.Search(specification), cancellationToken);

        if (specification.ShouldIncludePermissions)
        {
            foreach (var session in sessions)
            {
                session.Permissions = (await _queryMapper.MapValuesAsync<string>(_queryFactory.GetPermissions(session.IdentityId), cancellationToken)).ToList();
            }
        }

        return sessions;
    }
}