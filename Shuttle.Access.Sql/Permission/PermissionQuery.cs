using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class PermissionQuery : IPermissionQuery
    {
        private readonly IPermissionQueryFactory _queryFactory;
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IQueryMapper _queryMapper;

        public PermissionQuery(IDatabaseGateway databaseGateway, IQueryMapper queryMapper, IPermissionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryMapper, nameof(queryMapper));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public async Task<IEnumerable<DataAccess.Query.Permission>> SearchAsync(DataAccess.Query.Permission.Specification specification, CancellationToken cancellationToken = default)
        {
            return await _queryMapper.MapObjectsAsync<DataAccess.Query.Permission>(_queryFactory.Search(specification), cancellationToken);
        }

        public async ValueTask<int> CountAsync(DataAccess.Query.Permission.Specification specification, CancellationToken cancellationToken = default)
        {
            return await _databaseGateway.GetScalarAsync<int>(_queryFactory.Count(specification), cancellationToken);
        }

        public async ValueTask<bool> ContainsAsync(DataAccess.Query.Permission.Specification specification, CancellationToken cancellationToken = default)
        {
            return await _databaseGateway.GetScalarAsync<int>(_queryFactory.Contains(specification), cancellationToken) == 1;
        }
    }
}