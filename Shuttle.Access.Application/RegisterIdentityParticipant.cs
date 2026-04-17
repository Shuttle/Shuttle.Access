using Shuttle.Mediator;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

public class RegisterIdentityParticipant(IEventStore eventStore, IIdKeyRepository idKeyRepository, ITenantQuery tenantQuery) : IParticipant<RegisterIdentity>
{
    public async Task HandleAsync(RegisterIdentity message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(eventStore);
        ArgumentNullException.ThrowIfNull(idKeyRepository);
        ArgumentNullException.ThrowIfNull(tenantQuery);

        var key = Identity.Key(message.Name);

        EventStream stream;
        Identity aggregate;

        if (!await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            await idKeyRepository.AddAsync(message.Id, key, cancellationToken);

            stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustBeEmpty();
            aggregate = stream.Get<Identity>();

            var registered = aggregate.Register(message.Name, message.Description, message.PasswordHash, message.RegisteredBy, message.GeneratedPassword, message.Activated);

            stream.Add(registered);
        }
        else
        {
            stream = (await eventStore.GetAsync(message.Id, cancellationToken));
            aggregate = stream.Get<Identity>();
        }

        if (message.Activated && !aggregate.Activated)
        {
            stream.Add(aggregate.Activate(DateTimeOffset.UtcNow));
        }

        if (!message.HasTenantIds)
        {
            var specification = new Query.Tenant.Specification().IncludeActiveOnly();

            if (await tenantQuery.CountAsync(specification, cancellationToken) == 1)
            {
                message.AddTenantId((await tenantQuery.SearchAsync(specification, cancellationToken)).First().Id);
            }
        }

        foreach (var tenantId in message.TenantIds)
        {
            if (aggregate.IsInTenant(tenantId))
            {
                continue;
            }

            stream.Add(aggregate.AddTenant(tenantId));
        }

        foreach (var roleId in message.RoleIds)
        {
            if (aggregate.IsInRole(roleId))
            {
                continue;
            }

            stream.Add(aggregate.AddRole(roleId));
        }

        if (!stream.ShouldSave())
        {
            return;
        }

        await eventStore.SaveAsync(stream, cancellationToken);
    }
}