using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public class SystemRoleQuery : ISystemRoleQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ISystemRoleQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public SystemRoleQuery(IDatabaseGateway databaseGateway, ISystemRoleQueryFactory queryFactory,
            IQueryMapper queryMapper)
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

        public IEnumerable<DataRow> Search()
        {
            return _databaseGateway.GetRowsUsing(_queryFactory.Search());
        }

        public void Added(PrimitiveEvent primitiveEvent, Added domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.Added(primitiveEvent.Id, domainEvent));
        }

        public void PermissionAdded(PrimitiveEvent primitiveEvent, PermissionAdded domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.PermissionAdded(primitiveEvent.Id, domainEvent));
        }

        public void PermissionRemoved(PrimitiveEvent primitiveEvent, PermissionRemoved domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.PermissionRemoved(primitiveEvent.Id, domainEvent));
        }

        public void Removed(PrimitiveEvent primitiveEvent, Removed domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.Removed(primitiveEvent.Id, domainEvent));
        }

        public Query.Role Get(Guid id)
        {
            var result = _queryMapper.MapObject<Query.Role>(_queryFactory.Get(id));

            result.Permissions = new List<string>(_queryMapper.MapValues<string>(_queryFactory.Permissions(id)));

            return result;
        }

        public IEnumerable<string> Permissions(Guid id)
        {
            return _queryMapper.MapValues<string>(_queryFactory.Permissions(id));
        }
    }
}