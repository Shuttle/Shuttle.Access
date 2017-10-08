using System.Collections.Generic;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access.Sql
{
    public class PermissionQuery : IPermissionQuery
    {
        private readonly IPermissionQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public PermissionQuery(IQueryMapper queryMapper, IPermissionQueryFactory queryFactory)
        {
            Guard.AgainstNull(queryMapper, "queryMapper");
            Guard.AgainstNull(queryFactory, "queryFactory");

            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public IEnumerable<string> Available()
        {
            return _queryMapper.MapValues<string>(_queryFactory.Available());
        }
    }
}