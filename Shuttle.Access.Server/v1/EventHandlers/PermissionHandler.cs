using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class PermissionHandler(ILogger<PermissionHandler> logger, AccessDbContext accessDbContext)
    :
        IEventHandler<Registered>,
        IEventHandler<Activated>,
        IEventHandler<Deactivated>,
        IEventHandler<Removed>,
        IEventHandler<NameSet>,
        IEventHandler<DescriptionSet>
{
    private readonly ILogger<PermissionHandler> _logger = Guard.AgainstNull(logger);
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async Task ProcessEventAsync(IEventHandlerContext<Activated> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await SetStatusAsync(context.PrimitiveEvent.Id, (int)PermissionStatus.Deactivated, cancellationToken);

        _logger.LogDebug("[Activated] : id = '{PrimitiveEventId}'", context.PrimitiveEvent.Id);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Deactivated> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await SetStatusAsync(context.PrimitiveEvent.Id, (int)PermissionStatus.Deactivated, cancellationToken);

        _logger.LogDebug("[Deactivated] : id = '{PrimitiveEventId}'", context.PrimitiveEvent.Id);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<DescriptionSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = (await _accessDbContext.Permissions.FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(context.PrimitiveEvent.Id);

        model.Description = context.Event.Description;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[NameSet] : id = '{PrimitiveEventId}' / description = '{Description}'", context.PrimitiveEvent.Id, context.Event.Description);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = (await _accessDbContext.Permissions.FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(context.PrimitiveEvent.Id);

        model.Name = context.Event.Name;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[NameSet] : id = '{PrimitiveEventId}' / name = '{Name}'", context.PrimitiveEvent.Id, context.Event.Name);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        _accessDbContext.Permissions.Add(new()
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name,
            Description = context.Event.Description,
            Status = (int)context.Event.Status
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[Registered] : id = '{PrimitiveEventId}' / name = '{Name}' / status = '{PermissionStatus}'", context.PrimitiveEvent.Id, context.Event.Name, context.Event.Status);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await SetStatusAsync(context.PrimitiveEvent.Id, (int)PermissionStatus.Removed, cancellationToken);

        _logger.LogDebug("[Removed] : id = '{PrimitiveEventId}'", context.PrimitiveEvent.Id);
    }

    private async Task SetStatusAsync(Guid id, int status, CancellationToken cancellationToken)
    {
        var model = (await _accessDbContext.Permissions.FirstOrDefaultAsync(item => item.Id == id, cancellationToken))
            .GuardAgainstRecordNotFound(id);

        model.Status = status;

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }
}