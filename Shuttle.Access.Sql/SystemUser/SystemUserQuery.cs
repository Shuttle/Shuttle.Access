using System;
using System.Collections.Generic;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemUserQuery : ISystemUserQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ISystemUserQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public SystemUserQuery(IDatabaseGateway databaseGateway,
            IQueryMapper queryMapper, ISystemUserQueryFactory queryFactory)
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

        public IEnumerable<DataAccess.Query.User> Search(DataAccess.Query.User.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return _queryMapper.MapObjects<DataAccess.Query.User>(_queryFactory.Search(specification));
        }

        public DataAccess.Query.UserExtended GetExtended(Guid id)
        {
            var row = _databaseGateway.GetSingleRowUsing(_queryFactory.Get(id));

            var result = new DataAccess.Query.UserExtended
            {
                Id = SystemUserColumns.Id.MapFrom(row),
                DateRegistered = SystemUserColumns.DateRegistered.MapFrom(row),
                RegisteredBy = SystemUserColumns.RegisteredBy.MapFrom(row),
                Username = SystemUserColumns.Username.MapFrom(row)
            };

            foreach (var roleRow in _databaseGateway.GetRowsUsing(_queryFactory.Roles(id)))
            {
                result.Roles.Add(SystemUserRoleColumns.RoleId.MapFrom(roleRow));
            }

            return result;
        }

        public IEnumerable<Guid> Roles(Guid id)
        {
            return _queryMapper.MapValues<Guid>(_queryFactory.Roles(id));
        }

        public int Count(DataAccess.Query.User.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return _databaseGateway.GetScalarUsing<int>(_queryFactory.Count(specification));
        }
    }
}