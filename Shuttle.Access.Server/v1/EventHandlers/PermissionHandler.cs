using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;
using NameSet = Shuttle.Access.Events.Permission.v1.NameSet;
using Registered = Shuttle.Access.Events.Permission.v1.Registered;
using Removed = Shuttle.Access.Events.Permission.v1.Removed;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class PermissionHandler(ILogger<PermissionHandler> logger, AccessDbContext accessDbContext, IBus bus)
    :
        IEventHandler<Registered>,
        IEventHandler<Activated>,
        IEventHandler<Deactivated>,
        IEventHandler<Removed>,
        IEventHandler<NameSet>,
        IEventHandler<DescriptionSet>
{
    private readonly IBus _bus = Guard.AgainstNull(bus);
    private readonly ILogger<PermissionHandler> _logger = Guard.AgainstNull(logger);
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async Task HandleAsync(IEventHandlerContext<Activated> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(accessDbContext);

        await SetStatusAsync(context.PrimitiveEvent.Id, (int)PermissionStatus.Deactivated, cancellationToken);

        _logger.LogDebug("[Activated] : id = '{PrimitiveEventId}'", context.PrimitiveEvent.Id);
    }

    public async Task HandleAsync(IEventHandlerContext<Deactivated> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(accessDbContext);

        await SetStatusAsync(context.PrimitiveEvent.Id, (int)PermissionStatus.Deactivated, cancellationToken);

        _logger.LogDebug("[Deactivated] : id = '{PrimitiveEventId}'", context.PrimitiveEvent.Id);
    }

    public async Task HandleAsync(IEventHandlerContext<DescriptionSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(accessDbContext);

        var model = (await _accessDbContext.Permissions.FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(context.PrimitiveEvent.Id);

        model.Description = context.Event.Description;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[NameSet] : id = '{PrimitiveEventId}' / description = '{Description}'", context.PrimitiveEvent.Id, context.Event.Description);

        await _bus.PublishAsync(new PermissionDescriptionSet
        {
            Id = model.Id,
            Description = model.Description
        }, cancellationToken: cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<NameSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(accessDbContext);

        var model = (await _accessDbContext.Permissions.FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(context.PrimitiveEvent.Id);

        model.Name = context.Event.Name;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[NameSet] : id = '{PrimitiveEventId}' / name = '{Name}'", context.PrimitiveEvent.Id, context.Event.Name);

        await _bus.PublishAsync(new PermissionNameSet
        {
            Id = model.Id,
            Name = model.Name
        }, cancellationToken: cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(accessDbContext);

        _accessDbContext.Permissions.Add(new()
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name,
            Description = context.Event.Description,
            Status = (int)context.Event.Status
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[Registered] : id = '{PrimitiveEventId}' / name = '{Name}' / status = '{PermissionStatus}'", context.PrimitiveEvent.Id, context.Event.Name, context.Event.Status);

        await _bus.PublishAsync(new PermissionRegistered
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name,
            Description = context.Event.Description,
            Status = (int)context.Event.Status
        }, cancellationToken: cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<Removed> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(accessDbContext);

        await SetStatusAsync(context.PrimitiveEvent.Id, (int)PermissionStatus.Removed, cancellationToken);

        _logger.LogDebug("[Removed] : id = '{PrimitiveEventId}'", context.PrimitiveEvent.Id);

        await _bus.PublishAsync(new PermissionRemoved
        {
            PermissionId = context.PrimitiveEvent.Id
        }, cancellationToken: cancellationToken);
    }

    private async Task SetStatusAsync(Guid id, int status, CancellationToken cancellationToken)
    {
        var model = (await _accessDbContext.Permissions.FirstOrDefaultAsync(item => item.Id == id, cancellationToken))
            .GuardAgainstRecordNotFound(id);

        model.Status = status;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        await _bus.PublishAsync(new PermissionStatusSet
        {
            Id = model.Id,
            Name = model.Name,
            Status = model.Status
        }, cancellationToken: cancellationToken);
    }
}