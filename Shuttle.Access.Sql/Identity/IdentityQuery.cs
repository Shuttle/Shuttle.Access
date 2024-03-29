﻿using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class IdentityQuery : IIdentityQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IIdentityQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public IdentityQuery(IDatabaseGateway databaseGateway,
            IQueryMapper queryMapper, IIdentityQueryFactory queryFactory)
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
            return _databaseGateway.GetScalar<int>(_queryFactory.AdministratorCount());
        }

        public Guid Id(string identityName)
        {
            return _databaseGateway.GetScalar<Guid>(_queryFactory.GetId(identityName));
        }

        public IEnumerable<string> Permissions(Guid userId)
        {
            return _queryMapper.MapValues<string>(_queryFactory.Permissions(userId));
        }

        public IEnumerable<DataAccess.Query.Identity> Search(DataAccess.Query.Identity.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            var result = _queryMapper.MapObjects<DataAccess.Query.Identity>(_queryFactory.Search(specification));

            if (specification.RolesIncluded)
            {
                var roleRows = _databaseGateway.GetRows(_queryFactory.Roles(specification));

                foreach (var roleGroup in roleRows.GroupBy(row => Columns.IdentityId.MapFrom(row)))
                {
                    var user = result.FirstOrDefault(item => item.Id == roleGroup.Key);

                    if (user == null)
                    {
                        continue;
                    }

                    user.Roles = roleGroup.Select(row => new DataAccess.Query.Identity.Role
                        {Id = Columns.Id.MapFrom(row), Name = Columns.Name.MapFrom(row)}).ToList();
                }
            }

            return result;
        }

        public IEnumerable<Guid> RoleIds(DataAccess.Query.Identity.Specification specification)
        {
            return _databaseGateway.GetRows(_queryFactory.Roles(specification))
                .Select(row => Columns.RoleId.MapFrom(row));
        }

        public int Count(DataAccess.Query.Identity.Specification specification)
        {
            Guard.AgainstNull(specification, nameof(specification));

            return _databaseGateway.GetScalar<int>(_queryFactory.Count(specification));
        }
    }
}