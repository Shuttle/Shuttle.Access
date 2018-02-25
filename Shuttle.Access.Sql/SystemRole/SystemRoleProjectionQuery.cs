using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public class SystemRoleProjectionQuery : ISystemRoleProjectionQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ISystemRoleQueryFactory _queryFactory;

        public SystemRoleProjectionQuery(IDatabaseGateway databaseGateway, ISystemRoleQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
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
    }
}