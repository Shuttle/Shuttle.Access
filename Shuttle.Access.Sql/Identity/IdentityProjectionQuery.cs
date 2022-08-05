using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public class IdentityProjectionQuery : IIdentityProjectionQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IIdentityQueryFactory _queryFactory;

        public IdentityProjectionQuery(IDatabaseGateway databaseGateway, IIdentityQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
        }

        public void Register(PrimitiveEvent primitiveEvent, Registered domainEvent)
        {
            _databaseGateway.Execute(_queryFactory.Register(primitiveEvent.Id, domainEvent));
        }

        public void RoleAdded(PrimitiveEvent primitiveEvent, RoleAdded domainEvent)
        {
            _databaseGateway.Execute(_queryFactory.RoleAdded(primitiveEvent.Id, domainEvent));
        }

        public void RoleRemoved(PrimitiveEvent primitiveEvent, RoleRemoved domainEvent)
        {
            _databaseGateway.Execute(_queryFactory.RoleRemoved(primitiveEvent.Id, domainEvent));
        }

        public void Removed(PrimitiveEvent primitiveEvent)
        {
            _databaseGateway.Execute(_queryFactory.RemoveRoles(primitiveEvent.Id));
            _databaseGateway.Execute(_queryFactory.Remove(primitiveEvent.Id));
        }

        public void Activated(PrimitiveEvent primitiveEvent, Activated domainEvent)
        {
            _databaseGateway.Execute(_queryFactory.Activated(primitiveEvent.Id, domainEvent));
        }

        public void NameSet(PrimitiveEvent primitiveEvent, NameSet domainEvent)
        {
            _databaseGateway.Execute(_queryFactory.NameSet(primitiveEvent.Id, domainEvent));
        }
    }
}