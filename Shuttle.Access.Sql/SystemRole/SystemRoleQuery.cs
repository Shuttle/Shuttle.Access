using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemRoleQuery : ISystemRoleQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IDataRowMapper _dataRowMapper;
        private readonly ISystemRoleQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public SystemRoleQuery(IDatabaseGateway databaseGateway, IDataRowMapper dataRowMapper,
            IQueryMapper queryMapper, ISystemRoleQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(dataRowMapper, nameof(dataRowMapper));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));
            Guard.AgainstNull(queryMapper, nameof(queryMapper));

            _databaseGateway = databaseGateway;
            _dataRowMapper = dataRowMapper;
            _queryFactory = queryFactory;
            _queryMapper = queryMapper;
        }

        public IEnumerable<string> Permissions(string roleName)
        {
            return
                _databaseGateway.GetRowsUsing(_queryFactory.Permissions(roleName))
                    .Select(row => Columns.Permission.MapFrom(row))
                    .ToList();
        }

        public IEnumerable<DataAccess.Query.Role> Search(DataAccess.Query.Role.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var result = _queryMapper.MapObjects<DataAccess.Query.Role>(_queryFactory.Search(specification)).ToList();

            if (specification.PermissionsIncluded)
            {
                var permissionRows = _databaseGateway.GetRowsUsing(_queryFactory.Permissions(specification));

                foreach (var permissionGroup in permissionRows.GroupBy(row=>Columns.RoleId.MapFrom(row)))
                {
                    var role = result.FirstOrDefault(item => item.Id == permissionGroup.Key);

                    if (role == null)
                    {
                        continue;
                    }

                    role.Permissions = permissionGroup.Select(row => Columns.Permission.MapFrom(row)).ToList();
                }
            }

            return result;
        }

        public IEnumerable<string> Permissions(Guid id)
        {
            return _queryMapper.MapValues<string>(_queryFactory.Permissions(id));
        }

        public int Count(DataAccess.Query.Role.Specification specification)
        {
            return _databaseGateway.GetScalarUsing<int>(_queryFactory.Count(specification));
        }
    }
}