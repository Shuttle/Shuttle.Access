using System.Transactions;
using Shuttle.Access.Application;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.WebApi.Handlers;

public class AccessServiceHandler(IBus bus, IPrimitiveEventQuery primitiveEventQuery, IProjectionQuery projectionQuery, ISessionQuery sessionQuery, IMediator mediator)
    :
        IMessageHandler<IdentityRoleAdded>,
        IMessageHandler<IdentityRoleRemoved>,
        IMessageHandler<RolePermissionAdded>,
        IMessageHandler<RolePermissionRemoved>,
        IMessageHandler<PermissionStatusSet>
{
    private readonly IProjectionQuery _projectionQuery = Guard.AgainstNull(projectionQuery);
    private readonly IBus _bus = Guard.AgainstNull(bus);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly IPrimitiveEventQuery _primitiveEventQuery = Guard.AgainstNull(primitiveEventQuery);
    private readonly ISessionQuery _sessionQuery = Guard.AgainstNull(sessionQuery);

    public async Task HandleAsync(IdentityRoleAdded message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (await ShouldDeferAsync(cancellationToken))
        {
            await _bus.SendAsync(message, builder => builder.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new SessionSpecification().WithIdentityId(message.IdentityId));
    }

    private async Task<bool> ShouldDeferAsync(CancellationToken cancellationToken)
    {
        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
        {
            var sequenceNumber = await _primitiveEventQuery.GetMaximumSequenceNumberAsync(new(), cancellationToken);

            return sequenceNumber == null || await _projectionQuery.HasPendingProjectionsAsync(sequenceNumber.Value, cancellationToken);
        }
    }

    public async Task HandleAsync(PermissionStatusSet message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (await ShouldDeferAsync(cancellationToken))
        {
            await _bus.SendAsync(message, c => c.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new SessionSpecification().AddPermission(message.Name));
    }

    public async Task HandleAsync(RolePermissionAdded message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (await ShouldDeferAsync(cancellationToken))
        {
            await _bus.SendAsync(message, c => c.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new SessionSpecification().WithRoleId(message.RoleId));
    }

    private async Task RefreshAsync(SessionSpecification sessionSpecification)
    {
        foreach (var session in await _sessionQuery.SearchAsync(sessionSpecification))
        {
            await _mediator.SendAsync(new RefreshSession(session.Id));
        }
    }

    public async Task HandleAsync(IdentityRoleRemoved message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (await ShouldDeferAsync(cancellationToken))
        {
            await _bus.SendAsync(message, builder => builder.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new SessionSpecification().WithRoleId(message.RoleId));
    }

    public async Task HandleAsync(RolePermissionRemoved message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (await ShouldDeferAsync(cancellationToken))
        {
            await _bus.SendAsync(message, builder => builder.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new SessionSpecification().WithRoleId(message.RoleId));
    }
}