using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.User.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public class SystemUserProjectionQuery : ISystemUserProjectionQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ISystemUserQueryFactory _queryFactory;

        public SystemUserProjectionQuery(IDatabaseGateway databaseGateway, ISystemUserQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
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

        public void Removed(PrimitiveEvent primitiveEvent, Removed domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.RemoveRoles(primitiveEvent.Id, domainEvent));
            _databaseGateway.ExecuteUsing(_queryFactory.Remove(primitiveEvent.Id, domainEvent));
        }

        public void Activated(PrimitiveEvent primitiveEvent, Activated domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.Activated(primitiveEvent.Id, domainEvent));
        }
    }
}