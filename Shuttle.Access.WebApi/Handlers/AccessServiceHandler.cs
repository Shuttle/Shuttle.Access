using Shuttle.Access.Application;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.WebApi.Handlers;

public class AccessServiceHandler(IPrimitiveEventQuery primitiveEventQuery, IProjectionRepository projectionRepository, IPermissionQuery permissionQuery, ISessionQuery sessionQuery, IMediator mediator)
    :
        IMessageHandler<IdentityRoleSet>,
        IMessageHandler<RolePermissionSet>,
        IMessageHandler<PermissionStatusSet>
{
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly IPermissionQuery _permissionQuery = Guard.AgainstNull(permissionQuery);
    private readonly IPrimitiveEventQuery _primitiveEventQuery = Guard.AgainstNull(primitiveEventQuery);
    private readonly IProjectionRepository _projectionRepository = Guard.AgainstNull(projectionRepository);
    private readonly ISessionQuery _sessionQuery = Guard.AgainstNull(sessionQuery);

    public async Task ProcessMessageAsync(IHandlerContext<IdentityRoleSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        var primitiveEvent = (await _primitiveEventQuery.SearchAsync(new PrimitiveEvent.Specification().AddId(message.IdentityId).AddVersion(message.Version), cancellationToken)).FirstOrDefault();

        if (primitiveEvent?.SequenceNumber == null ||
            (await _projectionRepository.GetAsync(ProjectionNames.Identity, cancellationToken)).SequenceNumber < primitiveEvent.SequenceNumber.Value)
        {
            await context.SendAsync(message, c => c.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new SessionSpecification().AddPermissions(
            (await _permissionQuery.SearchAsync(new PermissionSpecification().AddRoleId(message.RoleId), cancellationToken))
            .Select(item => item.Name)));
    }

    public async Task ProcessMessageAsync(IHandlerContext<PermissionStatusSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        var primitiveEvent = (await _primitiveEventQuery.SearchAsync(new PrimitiveEvent.Specification().AddId(message.Id).AddVersion(message.Version), cancellationToken)).FirstOrDefault();

        if (primitiveEvent?.SequenceNumber == null ||
            (await _projectionRepository.GetAsync(ProjectionNames.Identity, cancellationToken)).SequenceNumber < primitiveEvent.SequenceNumber.Value)
        {
            await context.SendAsync(message, c => c.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new SessionSpecification().AddPermission(message.Name));
    }

    public async Task ProcessMessageAsync(IHandlerContext<RolePermissionSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        var primitiveEvent = (await _primitiveEventQuery.SearchAsync(new PrimitiveEvent.Specification().AddId(message.RoleId).AddVersion(message.Version), cancellationToken)).FirstOrDefault();

        if (primitiveEvent?.SequenceNumber == null ||
            (await _projectionRepository.GetAsync(ProjectionNames.Identity, cancellationToken)).SequenceNumber < primitiveEvent.SequenceNumber.Value)
        {
            await context.SendAsync(message, c => c.DeferUntil(DateTime.UtcNow.AddSeconds(5)).ToSelf(), cancellationToken);

            return;
        }

        await RefreshAsync(new SessionSpecification().AddPermissions(
            (await _permissionQuery.SearchAsync(new PermissionSpecification().AddId(message.PermissionId), cancellationToken))
            .Select(item => item.Name)));
    }

    private async Task RefreshAsync(SessionSpecification sessionSpecification)
    {
        foreach (var session in await _sessionQuery.SearchAsync(sessionSpecification))
        {
            await _mediator.SendAsync(new RefreshSession(session.Id));
        }
    }
}