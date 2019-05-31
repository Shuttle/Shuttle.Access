﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class SystemRoleQuery : ISystemRoleQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ISystemRoleQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public SystemRoleQuery(IDatabaseGateway databaseGateway,
            IQueryMapper queryMapper, ISystemRoleQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));
            Guard.AgainstNull(queryMapper, nameof(queryMapper));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
            _queryMapper = queryMapper;
        }

        public IEnumerable<string> Permissions(string roleName)
        {
            return
                _databaseGateway.GetRowsUsing(_queryFactory.Permissions(roleName))
                    .Select(row => SystemRolePermissionColumns.Permission.MapFrom(row))
                    .ToList();
        }

        public IEnumerable<DataRow> Search(DataAccess.Query.Role.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return _databaseGateway.GetRowsUsing(_queryFactory.Search(specification));
        }

        public DataAccess.Query.Role Get(Guid id)
        {
            var result = _queryMapper.MapObject<DataAccess.Query.Role>(_queryFactory.Get(id));

            if (result == null)
            {
                throw RecordNotFoundException.For<DataAccess.Query.Role>(id);
            }

            result.Permissions = new List<string>(_queryMapper.MapValues<string>(_queryFactory.Permissions(id)));

            return result;
        }

        public IEnumerable<string> Permissions(Guid id)
        {
            return _queryMapper.MapValues<string>(_queryFactory.Permissions(id));
        }
    }
}