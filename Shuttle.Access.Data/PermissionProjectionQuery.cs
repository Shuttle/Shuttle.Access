using Microsoft.EntityFrameworkCore;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Data;

public class PermissionProjectionQuery(AccessDbContext accessDbContext) : IPermissionProjectionQuery
{
    private readonly AccessDbContext _accessDbContext = Guard.AgainstNull(accessDbContext);

    public async Task ActivatedAsync(PrimitiveEvent primitiveEvent, Activated domainEvent, CancellationToken cancellationToken = default)
    {
        await SetStatusAsync(primitiveEvent.Id, (int)PermissionStatus.Active, cancellationToken);
    }

    public async Task DeactivatedAsync(PrimitiveEvent primitiveEvent, Deactivated domainEvent, CancellationToken cancellationToken = default)
    {
        await SetStatusAsync(primitiveEvent.Id, (int)PermissionStatus.Deactivated, cancellationToken);
    }

    public async Task NameSetAsync(PrimitiveEvent primitiveEvent, NameSet domainEvent, CancellationToken cancellationToken = default)
    {
        var model = (await _accessDbContext.Permissions.FirstOrDefaultAsync(item => item.Id == primitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(primitiveEvent.Id);

        model.Name = domainEvent.Name;

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisteredAsync(PrimitiveEvent primitiveEvent, Registered domainEvent, CancellationToken cancellationToken = default)
    {
        _accessDbContext.Permissions.Add(new()
        {
            Id = primitiveEvent.Id,
            Name = domainEvent.Name,
            Description = domainEvent.Description,
            Status = (int)domainEvent.Status
        });

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovedAsync(PrimitiveEvent primitiveEvent, Removed domainEvent, CancellationToken cancellationToken = default)
    {
        await SetStatusAsync(primitiveEvent.Id, (int)PermissionStatus.Removed, cancellationToken);
    }

    public async Task DescriptionSetAsync(PrimitiveEvent primitiveEvent, DescriptionSet domainEvent, CancellationToken cancellationToken = default)
    {
        var model = (await _accessDbContext.Permissions.FirstOrDefaultAsync(item => item.Id == primitiveEvent.Id, cancellationToken))
            .GuardAgainstRecordNotFound(primitiveEvent.Id);

        model.Description = domainEvent.Description;

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SetStatusAsync(Guid id, int status, CancellationToken cancellationToken)
    {
        var model = (await _accessDbContext.Permissions.FirstOrDefaultAsync(item => item.Id == id, cancellationToken))
            .GuardAgainstRecordNotFound(id);

        model.Status = status;

        await _accessDbContext.SaveChangesAsync(cancellationToken);
    }
}