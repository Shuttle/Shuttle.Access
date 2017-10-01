using System;
using System.Collections.Generic;
using System.Data;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access.Sql
{
    public class SystemUserQuery : ISystemUserQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ISystemUserQueryFactory _queryFactory;
        private readonly IQueryMapper _queryMapper;

        public SystemUserQuery(IDatabaseGateway databaseGateway, ISystemUserQueryFactory queryFactory, IQueryMapper queryMapper)
        {
            Guard.AgainstNull(databaseGateway, "databaseGateway");
            Guard.AgainstNull(queryFactory, "queryFactory");
            Guard.AgainstNull(queryMapper, "queryMapper");

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
            _queryMapper = queryMapper;
        }

        public void Register(PrimitiveEvent primitiveEvent, Registered domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.Register(primitiveEvent.Id, domainEvent));
        }

        public void RoleAdded(PrimitiveEvent primitiveEvent, RoleAdded domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.RoleAdded(primitiveEvent.Id, domainEvent));
        }

        public void RoleRemoved(PrimitiveEvent primitiveEvent, RoleRemoved domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.RoleRemoved(primitiveEvent.Id, domainEvent));
        }

        public int AdministratorCount()
        {
            return _databaseGateway.GetScalarUsing<int>(_queryFactory.AdministratorCount());
        }

        public void Removed(PrimitiveEvent primitiveEvent, Removed domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.RemoveRoles(primitiveEvent.Id, domainEvent));
            _databaseGateway.ExecuteUsing(_queryFactory.Remove(primitiveEvent.Id, domainEvent));
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