using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall;

namespace Shuttle.Access.Sql;

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

    public async Task RegisteredAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseGateway.ExecuteAsync(_queryFactory.Registered(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task PermissionAddedAsync(PrimitiveEvent primitiveEvent, PermissionAdded domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseGateway.ExecuteAsync(_queryFactory.PermissionAdded(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task PermissionRemovedAsync(PrimitiveEvent primitiveEvent, PermissionRemoved domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseGateway.ExecuteAsync(_queryFactory.PermissionRemoved(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task RemovedAsync(PrimitiveEvent primitiveEvent, CancellationToken cancellationToken = default)
    {
        await _databaseGateway.ExecuteAsync(_queryFactory.Removed(primitiveEvent.Id), cancellationToken);
    }

    public async Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseGateway.ExecuteAsync(_queryFactory.NameSet(primitiveEvent.Id, domainEvent), cancellationToken);
    }
}