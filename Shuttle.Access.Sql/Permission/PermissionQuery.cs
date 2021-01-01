using System.Collections.Generic;
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

        public IEnumerable<string> Available()
        {
            return _queryMapper.MapValues<string>(_queryFactory.Available());
        }

        public void Register(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, nameof(permission));

            _databaseGateway.ExecuteUsing(_queryFactory.Register(permission));
        }

        public void Remove(string permission)
        {
            Guard.AgainstNullOrEmptyString(permission, nameof(permission));

            _databaseGateway.ExecuteUsing(_queryFactory.Remove(permission));
        }
    }
}