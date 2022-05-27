using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall;

namespace Shuttle.Access.Sql
{
    public class PermissionProjectionQuery : IPermissionProjectionQuery
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IPermissionQueryFactory _queryFactory;

        public PermissionProjectionQuery(IDatabaseGateway databaseGateway, IPermissionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
        }

        public void Registered(PrimitiveEvent primitiveEvent, Registered domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.Registered(primitiveEvent.Id, domainEvent));
        }

        public void Activated(PrimitiveEvent primitiveEvent, Activated domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.Activated(primitiveEvent.Id, domainEvent));
        }

        public void Deactivated(PrimitiveEvent primitiveEvent, Deactivated domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.Deactivated(primitiveEvent.Id, domainEvent));
        }

        public void Removed(PrimitiveEvent primitiveEvent, Removed domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.Removed(primitiveEvent.Id, domainEvent));
        }

        public void NameSet(PrimitiveEvent primitiveEvent, NameSet domainEvent)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.NameSet(primitiveEvent.Id, domainEvent));
        }
    }
}