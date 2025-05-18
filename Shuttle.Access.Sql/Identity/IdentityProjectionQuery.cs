using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall;

namespace Shuttle.Access.Sql;

public class IdentityProjectionQuery : IIdentityProjectionQuery
{
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IIdentityQueryFactory _queryFactory;

    public IdentityProjectionQuery(IDatabaseContextService databaseContextService, IIdentityQueryFactory queryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _queryFactory = Guard.AgainstNull(queryFactory);
    }

    public async Task RegisterAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.Register(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task RoleAddedAsync(PrimitiveEvent primitiveEvent, RoleAdded domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.RoleAdded(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task RoleRemovedAsync(PrimitiveEvent primitiveEvent, RoleRemoved domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.RoleRemoved(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task RemovedAsync(PrimitiveEvent primitiveEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.RemoveRoles(primitiveEvent.Id), cancellationToken);
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.Remove(primitiveEvent.Id), cancellationToken);
    }

    public async Task ActivatedAsync(PrimitiveEvent primitiveEvent, Activated domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.Activated(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task DescriptionSetAsync(PrimitiveEvent primitiveEvent, DescriptionSet domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.DescriptionSet(primitiveEvent.Id, domainEvent), cancellationToken);
    }

    public async Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.NameSet(primitiveEvent.Id, domainEvent), cancellationToken);
    }
}