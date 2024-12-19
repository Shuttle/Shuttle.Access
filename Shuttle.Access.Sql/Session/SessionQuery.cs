using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.DataAccess.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SessionQuery : ISessionQuery
    {
        private readonly IDatabaseContextService _databaseContextService;
        private readonly IQueryMapper _queryMapper;
        private readonly ISessionQueryFactory _queryFactory;

        public SessionQuery(IDatabaseContextService databaseContextService, IQueryMapper queryMapper, ISessionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseContextService);
            Guard.AgainstNull(queryMapper);
            Guard.AgainstNull(queryFactory);

            _databaseContextService = databaseContextService;
            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public async ValueTask<bool> ContainsAsync(Guid token, CancellationToken cancellationToken = default)
        {
            return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Contains(token), cancellationToken) == 1;
        }

        public async ValueTask<bool> ContainsAsync(Guid token, string permission, CancellationToken cancellationToken = default)
        {
            return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Contains(token, permission), cancellationToken) == 1;
        }

        public async Task<Messages.v1.Session> GetAsync(Guid token, CancellationToken cancellationToken = default)
        {
            var result = await _queryMapper.MapObjectAsync<Messages.v1.Session>(_queryFactory.Get(token), cancellationToken);

            result.GuardAgainstRecordNotFound(token);

            result.Permissions = (await _queryMapper.MapValuesAsync<string>(_queryFactory.GetPermissions(token), cancellationToken)).ToList();

            return result;
        }

        public async Task<IEnumerable<Messages.v1.Session>> SearchAsync(DataAccess.Query.Session.Specification specification, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(specification);

            return await _queryMapper.MapObjectsAsync<Messages.v1.Session>(_queryFactory.Search(specification), cancellationToken);
        }
    }
}