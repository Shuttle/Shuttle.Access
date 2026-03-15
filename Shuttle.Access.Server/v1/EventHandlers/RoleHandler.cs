using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class RoleHandler(ILogger<RoleHandler> logger, IOptions<AccessOptions> accessOptions, AccessDbContext accessDbContext, IPermissionQuery permissionQuery, IBus bus) :
    IEventHandler<Registered>,
    IEventHandler<Events.Role.v2.Registered>,
    IEventHandler<Removed>,
    IEventHandler<PermissionAdded>,
    IEventHandler<PermissionRemoved>,
    IEventHandler<NameSet>
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly IBus _bus = Guard.AgainstNull(bus);
    private readonly ILogger<RoleHandler> _logger = Guard.AgainstNull(logger);
    private readonly IPermissionQuery _permissionQuery = Guard.AgainstNull(permissionQuery);

    public async Task HandleAsync(IEventHandlerContext<NameSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = (await _accessDbContext.Roles.FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(context.PrimitiveEvent.Id);

        model.Name = context.Event.Name;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[NameSet] : id = '{PrimitiveEventId}' / name = '{Name}'", context.PrimitiveEvent.Id, context.Event.Name);

        await _bus.PublishAsync(new RoleNameSet
        {
            Id = model.Id,
            Name = model.Name
        }, cancellationToken: cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<PermissionAdded> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        if (!await _permissionQuery.ContainsAsync(new Query.Permission.Specification().AddId(context.Event.PermissionId), cancellationToken))
        {
            context.Defer();
            return;
        }

        var model = await _accessDbContext.RolePermissions
            .FirstOrDefaultAsync(item => item.RoleId == context.PrimitiveEvent.Id && item.PermissionId == context.Event.PermissionId, cancellationToken);

        if (model != null)
        {
            return;
        }

        var roleModel = await _accessDbContext.Roles.FirstAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken);

        model = new()
        {
            RoleId = context.PrimitiveEvent.Id,
            PermissionId = context.Event.PermissionId,
            TenantId = roleModel.TenantId
        };

        _accessDbContext.RolePermissions.Add(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[PermissionAdded] : id = '{PrimitiveEventId}' / permission id = '{PermissionId}'", context.PrimitiveEvent.Id, context.Event.PermissionId);

        await _bus.PublishAsync(new RolePermissionAdded
        {
            RoleId = model.RoleId,
            PermissionId = model.PermissionId
        }, cancellationToken: cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<PermissionRemoved> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = await _accessDbContext.RolePermissions.FirstOrDefaultAsync(item => item.RoleId == context.PrimitiveEvent.Id && item.PermissionId == context.Event.PermissionId, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.RolePermissions.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[PermissionRemoved] : id = '{PrimitiveEventId}' / permission id = '{PermissionId}'", context.PrimitiveEvent.Id, context.Event.PermissionId);

        await _bus.PublishAsync(new RolePermissionRemoved
        {
            RoleId = model.RoleId,
            PermissionId = model.PermissionId
        }, cancellationToken: cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        _accessDbContext.Roles.Add(new()
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name,
            TenantId = _accessOptions.SystemTenantId
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[Registered] : id = '{PrimitiveEventId}' / name = '{Name}'", context.PrimitiveEvent.Id, context.Event.Name);

        await _bus.PublishAsync(new RoleRegistered
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name
        }, cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<Events.Role.v2.Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        _accessDbContext.Roles.Add(new()
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name,
            TenantId = context.Event.TenantId
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[Registered] : id = '{PrimitiveEventId}' / name = '{Name}'", context.PrimitiveEvent.Id, context.Event.Name);

        await _bus.PublishAsync(new Messages.v2.RoleRegistered
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name,
            TenantId = context.Event.TenantId
        }, cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<Removed> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = await _accessDbContext.Roles.AsNoTracking().FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.Roles.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[Removed] : id = '{PrimitiveEventId}'", context.PrimitiveEvent.Id);

        await _bus.PublishAsync(new RoleRemoved
        {
            Id = model.Id,
            Name = model.Name
        }, cancellationToken: cancellationToken);
    }
}