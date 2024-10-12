using System.Threading;
using System.Threading.Tasks;
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

        public async Task RegisteredAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default)
        {
            await _databaseGateway.ExecuteAsync(_queryFactory.Registered(primitiveEvent.Id, domainEvent), cancellationToken);
        }

        public async Task ActivatedAsync(PrimitiveEvent primitiveEvent, Activated domainEvent, CancellationToken cancellationToken = default)
        {
            await _databaseGateway.ExecuteAsync(_queryFactory.Activated(primitiveEvent.Id, domainEvent), cancellationToken);
        }

        public async Task DeactivatedAsync(PrimitiveEvent primitiveEvent, Deactivated domainEvent, CancellationToken cancellationToken = default)
        {
            await _databaseGateway.ExecuteAsync(_queryFactory.Deactivated(primitiveEvent.Id, domainEvent), cancellationToken);
        }

        public async Task RemovedAsync(PrimitiveEvent primitiveEvent, Removed domainEvent, CancellationToken cancellationToken = default)
        {
            await _databaseGateway.ExecuteAsync(_queryFactory.Removed(primitiveEvent.Id, domainEvent), cancellationToken);
        }

        public async Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default)
        {
            await _databaseGateway.ExecuteAsync(_queryFactory.NameSet(primitiveEvent.Id, domainEvent), cancellationToken);
        }
    }
}