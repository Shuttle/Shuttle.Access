using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityTenantStatusParticipant(IEventStore eventStore, ITenantQuery tenantQuery, IIdentityQuery identityQuery) : IParticipant<SetIdentityTenantStatus>
{
    public async Task HandleAsync(SetIdentityTenantStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.IdentityId, cancellationToken);

        stream.Apply(identity);

        if (message.Active && !identity.IsInTenant(message.TenantId))
        {
            var tenant = (await tenantQuery.SearchAsync(new Query.Tenant.Specification().AddId(message.TenantId), cancellationToken)).FirstOrDefault();

            if (tenant is { MaximumIdentities: > 0 })
            {
                var identityCount = await identityQuery.CountAsync(new Query.Identity.Specification().WithTenantId(message.TenantId), cancellationToken);

                if (identityCount >= tenant.MaximumIdentities)
                {
                    throw new ApplicationException(string.Format(Access.Resources.TenantMaximumIdentitiesExceededException, tenant.Name, tenant.MaximumIdentities));
                }
            }

            stream.Add(identity.AddTenant(message.TenantId));
        }

        if (!message.Active && identity.IsInTenant(message.TenantId))
        {
            stream.Add(identity.RemoveTenant(message.TenantId));
        }

        if (stream.ShouldSave())
        {
            await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }
    }
}
