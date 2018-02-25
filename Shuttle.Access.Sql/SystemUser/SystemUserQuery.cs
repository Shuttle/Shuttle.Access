using System;
using System.Collections.Generic;
using System.Data;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemUserQuery : ISystemUserQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ISystemUserQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public SystemUserQuery(IDatabaseGateway databaseGateway, ISystemUserQueryFactory queryFactory,
            IQueryMapper queryMapper)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));
            Guard.AgainstNull(queryMapper, nameof(queryMapper));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
            _queryMapper = queryMapper;
        }

        public int AdministratorCount()
        {
            return _databaseGateway.GetScalarUsing<int>(_queryFactory.AdministratorCount());
        }

        public IEnumerable<DataRow> Search()
        {
            return _databaseGateway.GetRowsUsing(_queryFactory.Search());
        }

        public Query.User Get(Guid id)
        {
            var row = _databaseGateway.GetSingleRowUsing(_queryFactory.Get(id));

            var result = new Query.User
            {
                Id = SystemUserColumns.Id.MapFrom(row),
                DateRegistered = SystemUserColumns.DateRegistered.MapFrom(row),
                RegisteredBy = SystemUserColumns.RegisteredBy.MapFrom(row),
                Username = SystemUserColumns.Username.MapFrom(row)
            };

            foreach (var roleRow in _databaseGateway.GetRowsUsing(_queryFactory.Roles(id)))
            {
                result.Roles.Add(SystemUserRoleColumns.RoleName.MapFrom(roleRow));
            }

            return result;
        }

        public IEnumerable<string> Roles(Guid id)
        {
            return _queryMapper.MapValues<string>(_queryFactory.Roles(id));
        }

        public int Count()
        {
            return _databaseGateway.GetScalarUsing<int>(_queryFactory.Count());
        }
    }
}