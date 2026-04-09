using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shuttle.Access.Events.Tenant.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;
namespace Shuttle.Access.Server.v1.EventHandlers;

public class TenantHandler(ILogger<TenantHandler> logger, AccessDbContext accessDbContext, IBus bus) : 
    IEventHandler<Registered>,
    IEventHandler<Removed>,
    IEventHandler<StatusSet>
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);
    private readonly ILogger<TenantHandler> _logger = Guard.AgainstNull(logger);
    private readonly IBus _bus = Guard.AgainstNull(bus);

    public async Task HandleAsync(IEventHandlerContext<Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = await _accessDbContext.Tenants.FindAsync([context.PrimitiveEvent.Id], cancellationToken: cancellationToken);

        if (model != null)
        {
            return;
        }

        _accessDbContext.Tenants.Add(new()
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name,
            Status = context.Event.Status,
            LogoSvg = context.Event.LogoSvg,
            LogoUrl = context.Event.LogoUrl
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug($"[Registered] : id = '{context.PrimitiveEvent.Id}' / name = '{context.Event.Name}'");

        await _bus.PublishAsync(new TenantRegistered
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name
        }, cancellationToken: cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<StatusSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = await _accessDbContext.Tenants.FindAsync([context.PrimitiveEvent.Id], cancellationToken: cancellationToken);

        if (model == null)
        {
            return;
        }

        model.Status = (int)context.Event.Status;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        await _bus.PublishAsync(new TenantStatusSet
        {
            Id = model.Id,
            Name = model.Name,
            Status = model.Status
        }, cancellationToken: cancellationToken);
    }

    public async Task HandleAsync(IEventHandlerContext<Removed> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = await _accessDbContext.Tenants.AsNoTracking().FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.Tenants.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[Removed] : id = '{PrimitiveEventId}'", context.PrimitiveEvent.Id);

        await _bus.PublishAsync(new TenantRemoved
        {
            Id = model.Id,
            Name = model.Name
        }, cancellationToken: cancellationToken);
    }
}