using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.EventHandlers;

public class IdentityHandler(ILogger<IdentityHandler> logger, AccessDbContext accessDbContext, IRoleQuery roleQuery, ITenantQuery tenantQuery, ISessionRepository sessionRepository)
    :
        IEventHandler<Registered>,
        IEventHandler<RoleAdded>,
        IEventHandler<RoleRemoved>,
        IEventHandler<TenantAdded>,
        IEventHandler<TenantRemoved>,
        IEventHandler<Removed>,
        IEventHandler<Activated>,
        IEventHandler<NameSet>,
        IEventHandler<DescriptionSet>
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);
    private readonly ILogger<IdentityHandler> _logger = Guard.AgainstNull(logger);
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);
    private readonly ISessionRepository _sessionRepository = Guard.AgainstNull(sessionRepository);
    private readonly ITenantQuery _tenantQuery = Guard.AgainstNull(tenantQuery);

    public async Task ProcessEventAsync(IEventHandlerContext<Activated> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = (await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(context.PrimitiveEvent.Id);

        model.DateActivated = context.Event.DateActivated;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[Activated] : id = '{PrimitiveEventId}' / date activated = '{DateActivated:O}'", context.PrimitiveEvent.Id, context.Event.DateActivated);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<DescriptionSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = (await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(context.PrimitiveEvent.Id);

        model.Description = context.Event.Description;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[NameSet] : id = '{PrimitiveEventId}' / description = '{Description}'", context.PrimitiveEvent.Id, context.Event.Description);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<NameSet> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = (await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(context.PrimitiveEvent.Id);

        model.Name = context.Event.Name;

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[NameSet] : id = '{PrimitiveEventId}' / name = '{Name}'", context.PrimitiveEvent.Id, context.Event.Name);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Registered> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        _accessDbContext.Identities.Add(new()
        {
            Id = context.PrimitiveEvent.Id,
            Name = context.Event.Name,
            Description = context.Event.Description,
            DateRegistered = context.Event.DateRegistered,
            RegisteredBy = context.Event.RegisteredBy,
            GeneratedPassword = context.Event.GeneratedPassword
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[Registered] : id = '{PrimitiveEventId}' / name = '{Name}' / activated = '{Activated}' / date registered = '{DateRegistered}' / registered by = '{RegisteredBy}'", context.PrimitiveEvent.Id, context.Event.Name, context.Event.Activated, context.Event.DateRegistered, context.Event.RegisteredBy);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<Removed> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        await _sessionRepository.RemoveAsync(new SessionSpecification().AddId(context.PrimitiveEvent.Id), cancellationToken);

        var model = await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Id == context.PrimitiveEvent.Id, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.Identities.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[Removed] : id = '{PrimitiveEventId}'", context.PrimitiveEvent.Id);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleAdded> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var roleModel = (await _roleQuery.SearchAsync(new RoleSpecification().AddId(context.Event.RoleId), cancellationToken)).FirstOrDefault();

        if (roleModel == null ||
            !await _tenantQuery.ContainsAsync(new TenantSpecification().AddId(roleModel.TenantId), cancellationToken))
        {
            context.Defer();
            return;
        }

        var model = await _accessDbContext.IdentityRoles.FirstOrDefaultAsync(item => item.IdentityId == context.PrimitiveEvent.Id && item.RoleId == context.Event.RoleId, cancellationToken);

        if (model != null)
        {
            return;
        }

        var identityTenantModel = await _accessDbContext.IdentityTenants.FirstOrDefaultAsync(item => item.IdentityId == context.PrimitiveEvent.Id && item.TenantId == roleModel.TenantId, cancellationToken);

        if (identityTenantModel == null)
        {
            _accessDbContext.IdentityTenants.Add(new()
            {
                IdentityId = context.PrimitiveEvent.Id,
                TenantId = roleModel.TenantId,
                Status = 1
            });
        }

        _accessDbContext.IdentityRoles.Add(new()
        {
            IdentityId = context.PrimitiveEvent.Id,
            RoleId = context.Event.RoleId,
            TenantId = roleModel.TenantId
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[RoleAdded] : id = '{PrimitiveEventId}' / role id = '{RoleId}'", context.PrimitiveEvent.Id, context.Event.RoleId);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<RoleRemoved> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = await _accessDbContext.IdentityRoles.FirstOrDefaultAsync(item => item.IdentityId == context.PrimitiveEvent.Id && item.RoleId == context.Event.RoleId, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.IdentityRoles.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[RoleRemoved] : id = '{PrimitiveEventId}' / role id = '{RoleId}'", context.PrimitiveEvent.Id, context.Event.RoleId);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<TenantAdded> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var tenantModel = (await _tenantQuery.SearchAsync(new TenantSpecification().AddId(context.Event.TenantId), cancellationToken)).FirstOrDefault();

        if (tenantModel == null)
        {
            context.Defer();
            return;
        }

        _accessDbContext.IdentityTenants.Add(new()
        {
            IdentityId = context.PrimitiveEvent.Id,
            TenantId = context.Event.TenantId
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[TenantAdded] : id = '{PrimitiveEventId}' / tenant id = '{TenantId}'", context.PrimitiveEvent.Id, context.Event.TenantId);
    }

    public async Task ProcessEventAsync(IEventHandlerContext<TenantRemoved> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var model = await _accessDbContext.IdentityTenants.FirstOrDefaultAsync(item => item.IdentityId == context.PrimitiveEvent.Id && item.TenantId == context.Event.TenantId, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.IdentityTenants.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("[TenantRemoved] : id = '{PrimitiveEventId}' / tenant id = '{TenantId}'", context.PrimitiveEvent.Id, context.Event.TenantId);
    }
}