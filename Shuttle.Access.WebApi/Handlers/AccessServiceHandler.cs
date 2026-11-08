using System.Transactions;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.WebApi.Handlers;

public class AccessServiceHandler(IBus bus, IPrimitiveEventQuery primitiveEventQuery, IProjectionQuery projectionQuery, ISessionQuery sessionQuery, IMediator mediator)
    :
        IMessageHandler<IdentityRoleAdded>,
        IMessageHandler<IdentityRoleRemoved>,
        IMessageHandler<RolePermissionAdded>,
        IMessageHandler<RolePermissionRemoved>,
        IMessageHandler<PermissionStatusSet>
{
    private static readonly TimeSpan DeferDuration = TimeSpan.FromMilliseconds(200);

    private readonly IProjectionQuery _projectionQuery = Guard.AgainstNull(projectionQuery);
    private readonly IBus _bus = Guard.AgainstNull(bus);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly IPrimitiveEventQuery _primitiveEventQuery = Guard.AgainstNull(primitiveEventQuery);
    private readonly ISessionQuery _sessionQuery = Guard.AgainstNull(sessionQuery);

    public async Task HandleAsync(IdentityRoleAdded message, CancellationToken cancellationToken = default)
    {
        await RefreshAsync(Guard.AgainstNull(message), new Query.Session.Specification().WithIdentityId(message.IdentityId), cancellationToken);
    }

    public async Task HandleAsync(IdentityRoleRemoved message, CancellationToken cancellationToken = default)
    {
        // by identity: once the role has been removed the identity's sessions can no longer be found by the role
        await RefreshAsync(Guard.AgainstNull(message), new Query.Session.Specification().WithIdentityId(message.IdentityId), cancellationToken);
    }

    public async Task HandleAsync(PermissionStatusSet message, CancellationToken cancellationToken = default)
    {
        await RefreshAsync(Guard.AgainstNull(message), new Query.Session.Specification().AddPermissions([message.Name]), cancellationToken);
    }

    public async Task HandleAsync(RolePermissionAdded message, CancellationToken cancellationToken = default)
    {
        await RefreshAsync(Guard.AgainstNull(message), new Query.Session.Specification().WithRoleId(message.RoleId), cancellationToken);
    }

    public async Task HandleAsync(RolePermissionRemoved message, CancellationToken cancellationToken = default)
    {
        await RefreshAsync(Guard.AgainstNull(message), new Query.Session.Specification().WithRoleId(message.RoleId), cancellationToken);
    }

    private async Task RefreshAsync<T>(T message, Query.Session.Specification sessionSpecification, CancellationToken cancellationToken) where T : class
    {
        var sessions = (await _sessionQuery.SearchAsync(sessionSpecification, cancellationToken)).ToList();

        // Nothing to refresh, which is always the case while projections are being rebuilt (sessions are cleared), so
        // there is no reason to wait for the projections to catch up; deferring would only re-check them every few
        // seconds for every such message.
        if (sessions.Count == 0)
        {
            return;
        }

        if (await ShouldDeferAsync(cancellationToken))
        {
            await _bus.SendAsync(message, builder => builder.DeferUntil(DateTime.UtcNow.Add(DeferDuration)).ToSelf(), cancellationToken);

            return;
        }

        foreach (var session in sessions)
        {
            await _mediator.SendAsync(new RefreshSession(session.Id), cancellationToken);

            await _bus.PublishAsync(new SessionRefreshed
            {
                Id = session.Id,
                IdentityId = session.IdentityId,
                IdentityName = session.IdentityName
            }, cancellationToken);
        }
    }

    private async Task<bool> ShouldDeferAsync(CancellationToken cancellationToken)
    {
        using (new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
        {
            var sequenceNumber = await _primitiveEventQuery.GetMaximumSequenceNumberAsync(new(), cancellationToken);

            return sequenceNumber == null || await _projectionQuery.HasPendingProjectionsAsync(sequenceNumber.Value, cancellationToken);
        }
    }
}
