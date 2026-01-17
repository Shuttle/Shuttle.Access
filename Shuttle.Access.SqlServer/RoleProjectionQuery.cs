using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.SqlServer;

public class RoleProjectionQuery(IOptions<AccessOptions> accessOptions, AccessDbContext accessDbContext) : IRoleProjectionQuery
{
    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default)
    {
        var model = (await _accessDbContext.Roles.FirstOrDefaultAsync(item => item.Id == primitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(primitiveEvent.Id);

        model.Name = domainEvent.Name;

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PermissionAddedAsync(PrimitiveEvent primitiveEvent, PermissionAdded domainEvent, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.RolePermissions
            .FirstOrDefaultAsync(item => item.RoleId == primitiveEvent.Id && item.PermissionId == domainEvent.PermissionId, cancellationToken);

        if (model != null)
        {
            return;
        }

        var roleModel = await _accessDbContext.Roles.FirstAsync(item => item.Id == primitiveEvent.Id, cancellationToken: cancellationToken);

        _accessDbContext.RolePermissions.Add(new()
        {
            RoleId = primitiveEvent.Id,
            PermissionId = domainEvent.PermissionId,
            TenantId = roleModel.TenantId
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task PermissionRemovedAsync(PrimitiveEvent primitiveEvent, PermissionRemoved domainEvent, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.RolePermissions.FirstOrDefaultAsync(item => item.RoleId == primitiveEvent.Id && item.PermissionId == domainEvent.PermissionId, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.RolePermissions.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisteredAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default)
    {
        _accessDbContext.Roles.Add(new()
        {
            Id = primitiveEvent.Id,
            Name = domainEvent.Name,
            TenantId = _accessOptions.SystemTenantId
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisteredAsync(PrimitiveEvent primitiveEvent, Events.Role.v2.Registered domainEvent, CancellationToken cancellationToken = default)
    {
        _accessDbContext.Roles.Add(new()
        {
            Id = primitiveEvent.Id,
            Name = domainEvent.Name,
            TenantId = domainEvent.TenantId
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovedAsync(PrimitiveEvent primitiveEvent, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.Roles.AsNoTracking().FirstOrDefaultAsync(item => item.Id == primitiveEvent.Id, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.Roles.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }
}