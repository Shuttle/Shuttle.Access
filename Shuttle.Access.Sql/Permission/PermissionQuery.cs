using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.DataAccess.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class PermissionQuery : IPermissionQuery
    {
        private readonly IPermissionQueryFactory _queryFactory;
        private readonly IDatabaseContextService _databaseContextService;
        private readonly IQueryMapper _queryMapper;

        public PermissionQuery(IDatabaseContextService databaseContextService, IQueryMapper queryMapper, IPermissionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseContextService);
            Guard.AgainstNull(queryMapper);
            Guard.AgainstNull(queryFactory);

            _databaseContextService = databaseContextService;
            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public async Task<IEnumerable<Messages.v1.Permission>> SearchAsync(DataAccess.Query.Permission.Specification specification, CancellationToken cancellationToken = default)
        {
            return await _queryMapper.MapObjectsAsync<Messages.v1.Permission>(_queryFactory.Search(specification), cancellationToken);
        }

        public async ValueTask<int> CountAsync(DataAccess.Query.Permission.Specification specification, CancellationToken cancellationToken = default)
        {
            return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Count(specification), cancellationToken);
        }

        public async ValueTask<bool> ContainsAsync(DataAccess.Query.Permission.Specification specification, CancellationToken cancellationToken = default)
        {
            return await _databaseContextService.Active.GetScalarAsync<int>(_queryFactory.Contains(specification), cancellationToken) == 1;
        }
    }
}