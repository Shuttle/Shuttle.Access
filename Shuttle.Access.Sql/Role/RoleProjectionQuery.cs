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
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IRoleQueryFactory _queryFactory;

    public RoleProjectionQuery(IDatabaseContextService databaseContextService, IRoleQueryFactory queryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _queryFactory = Guard.AgainstNull(queryFactory);
    }

    public async Task RegisteredAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.Registered(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task PermissionAddedAsync(PrimitiveEvent primitiveEvent, PermissionAdded domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.PermissionAdded(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task PermissionRemovedAsync(PrimitiveEvent primitiveEvent, PermissionRemoved domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.PermissionRemoved(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task RemovedAsync(PrimitiveEvent primitiveEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.Removed(primitiveEvent.Id), cancellationToken);
    }

    public async Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.NameSet(primitiveEvent.Id, domainEvent), cancellationToken);
    }
}