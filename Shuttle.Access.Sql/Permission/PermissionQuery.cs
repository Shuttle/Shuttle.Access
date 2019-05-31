using System.Collections.Generic;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class PermissionQuery : IPermissionQuery
    {
        private readonly IPermissionQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public PermissionQuery(IQueryMapper queryMapper, IPermissionQueryFactory queryFactory)
        {
            Guard.AgainstNull(queryMapper, nameof(queryMapper));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public IEnumerable<string> Available()
        {
            return _queryMapper.MapValues<string>(_queryFactory.Available());
        }
    }
}