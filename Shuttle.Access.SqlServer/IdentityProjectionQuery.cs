using Microsoft.EntityFrameworkCore;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.SqlServer;

public class IdentityProjectionQuery(AccessDbContext accessDbContext) : IIdentityProjectionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async Task RegisterAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default)
    {
        _accessDbContext.Identities.Add(new()
        {
            Id = primitiveEvent.Id,
            Name = domainEvent.Name,
            Description = domainEvent.Description,
            DateRegistered = domainEvent.DateRegistered,
            RegisteredBy = domainEvent.RegisteredBy,
            GeneratedPassword = domainEvent.GeneratedPassword
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RoleAddedAsync(PrimitiveEvent primitiveEvent, RoleAdded domainEvent, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.IdentityRoles.FirstOrDefaultAsync(item => item.IdentityId == primitiveEvent.Id && item.RoleId == domainEvent.RoleId, cancellationToken);

        if (model != null)
        {
            return;
        }

        var roleModel = await _accessDbContext.Roles.FirstAsync(item => item.Id == domainEvent.RoleId, cancellationToken: cancellationToken);

        _accessDbContext.IdentityRoles.Add(new()
        {
            IdentityId = primitiveEvent.Id,
            RoleId = domainEvent.RoleId,
            TenantId = roleModel.TenantId
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RoleRemovedAsync(PrimitiveEvent primitiveEvent, RoleRemoved domainEvent, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.IdentityRoles.FirstOrDefaultAsync(item => item.IdentityId == primitiveEvent.Id && item.RoleId == domainEvent.RoleId, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.IdentityRoles.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovedAsync(PrimitiveEvent primitiveEvent, CancellationToken cancellationToken = default)
    {
        var model = await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Id == primitiveEvent.Id, cancellationToken);

        if (model == null)
        {
            return;
        }

        _accessDbContext.Identities.Remove(model);

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivatedAsync(PrimitiveEvent primitiveEvent, Activated domainEvent, CancellationToken cancellationToken = default)
    {
        var model = (await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Id == primitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(primitiveEvent.Id);

        model.DateActivated = domainEvent.DateActivated;

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DescriptionSetAsync(PrimitiveEvent primitiveEvent, DescriptionSet domainEvent, CancellationToken cancellationToken = default)
    {
        var model = (await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Id == primitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(primitiveEvent.Id);

        model.Description = domainEvent.Description;

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default)
    {
        var model = (await _accessDbContext.Identities.FirstOrDefaultAsync(item => item.Id == primitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(primitiveEvent.Id);

        model.Name = domainEvent.Name;

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }
}