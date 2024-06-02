using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SessionQuery : ISessionQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IQueryMapper _queryMapper;
        private readonly ISessionQueryFactory _queryFactory;

        public SessionQuery(IDatabaseGateway databaseGateway, IQueryMapper queryMapper, ISessionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryMapper, nameof(queryMapper));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryMapper = queryMapper;
            _queryFactory = queryFactory;
        }

        public ValueTask<bool> ContainsAsync(Guid token, CancellationToken cancellationToken = default)
        {
            return _databaseGateway.GetScalar<int>(_queryFactory.Contains(token)) == 1;
        }

        public ValueTask<bool> ContainsAsync(Guid token, string permission, CancellationToken cancellationToken = default)
        {
            return _databaseGateway.GetScalar<int>(_queryFactory.Contains(token, permission)) == 1;
        }

        public Task<DataAccess.Query.Session> GetAsync(Guid token, CancellationToken cancellationToken = default)
        {
            var result = _queryMapper.MapObject<DataAccess.Query.Session>(_queryFactory.Get(token));

            result.GuardAgainstRecordNotFound(token);

            result.Permissions = _queryMapper.MapValues<string>(_queryFactory.GetPermissions(token)).ToList();

            return result;
        }

        public Task<IEnumerable<Session>> SearchAsync(DataAccess.Query.Session.Specification specification, CancellationToken cancellationToken = default)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return _queryMapper.MapObjects<DataAccess.Query.Session>(_queryFactory.Search(specification));
        }
    }
}