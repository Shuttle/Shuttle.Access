using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public class RoleProjectionQuery : IRoleProjectionQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IRoleQueryFactory _queryFactory;

        public RoleProjectionQuery(IDatabaseGateway databaseGateway, IRoleQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
        }

        public void Registered(PrimitiveEvent primitiveEvent, Registered domainEvent)
        {
            _databaseGateway.Execute(_queryFactory.Registered(primitiveEvent.Id, domainEvent));
        }

        public void PermissionAdded(PrimitiveEvent primitiveEvent, PermissionAdded domainEvent)
        {
            _databaseGateway.Execute(_queryFactory.PermissionAdded(primitiveEvent.Id, domainEvent));
        }

        public void PermissionRemoved(PrimitiveEvent primitiveEvent, PermissionRemoved domainEvent)
        {
            _databaseGateway.Execute(_queryFactory.PermissionRemoved(primitiveEvent.Id, domainEvent));
        }

        public void Removed(PrimitiveEvent primitiveEvent)
        {
            _databaseGateway.Execute(_queryFactory.Removed(primitiveEvent.Id));
        }

        public void NameSet(PrimitiveEvent primitiveEvent, NameSet domainEvent)
        {
            _databaseGateway.Execute(_queryFactory.NameSet(primitiveEvent.Id, domainEvent));
        }
    }
}