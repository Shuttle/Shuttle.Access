using Shuttle.Core.Mediator;
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

        if (await idKeyRepository.ContainsAsync(key, cancellationToken))
        {
            return;
        }

        await idKeyRepository.AddAsync(message.Id, key, cancellationToken);

        var stream = (await eventStore.GetAsync(message.Id, cancellationToken)).MustBeEmpty();
        var aggregate = stream.Get<Identity>();

        var registered = aggregate.Register(message.Name, message.Description, message.PasswordHash, message.RegisteredBy, message.GeneratedPassword, message.Activated);

        stream.Add(registered);

        if (message.Activated)
        {
            stream.Add(aggregate.Activate(registered.DateRegistered));
        }

        if (message.HasTenantIds)
        {
            foreach (var tenantId in message.TenantIds)
            {
                stream.Add(aggregate.AddTenant(tenantId));
            }
        }
        else
        {
            var specification = new Query.Tenant.Specification().IncludeActiveOnly();

            if (await tenantQuery.CountAsync(specification, cancellationToken) == 1)
            {
                stream.Add(aggregate.AddTenant((await tenantQuery.SearchAsync(specification, cancellationToken)).First().Id));
            }
        }

        foreach (var roleId in message.RoleIds)
        {
            stream.Add(aggregate.AddRole(roleId));
        }

        await eventStore.SaveAsync(stream, cancellationToken);
    }
}