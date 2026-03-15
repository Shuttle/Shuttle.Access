using System.Transactions;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
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

        await RefreshAsync(new Query.Session.Specification().WithIdentityId(message.IdentityId), cancellationToken);
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

        await RefreshAsync(new Query.Session.Specification().AddPermission(message.Name), cancellationToken);
    }

    public async Task HandleAsync(RolePermissionAdded message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (await ShouldDeferAsync(cancellationToken))
        {
            await _bus.SendAsync(message, c => c.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new Query.Session.Specification().WithRoleId(message.RoleId), cancellationToken);
    }

    private async Task RefreshAsync(Query.Session.Specification sessionSpecification, CancellationToken cancellationToken)
    {
        foreach (var session in await _sessionQuery.SearchAsync(sessionSpecification, cancellationToken))
        {
            await _mediator.SendAsync(new RefreshSession(session.Id), cancellationToken);

            await _bus.PublishAsync(new SessionRefreshed
            {
                Id = session.Id,
                TenantId = session.TenantId,
                IdentityId = session.IdentityId,
                IdentityName = session.IdentityName
            }, cancellationToken);
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

        await RefreshAsync(new Query.Session.Specification().WithRoleId(message.RoleId), cancellationToken);
    }

    public async Task HandleAsync(RolePermissionRemoved message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (await ShouldDeferAsync(cancellationToken))
        {
            await _bus.SendAsync(message, builder => builder.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new Query.Session.Specification().WithRoleId(message.RoleId), cancellationToken);
    }
}