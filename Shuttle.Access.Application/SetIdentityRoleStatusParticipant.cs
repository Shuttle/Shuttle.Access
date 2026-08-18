using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityRoleStatusParticipant(IEventStore eventStore, IRoleQuery roleQuery, IMediator mediator) : IParticipant<SetIdentityRoleStatus>
{
    public async Task HandleAsync(SetIdentityRoleStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (!message.Active)
        {
            var review = new ReviewIdentityRoleRemoval(message.AuditTenantId, message.RoleId);

            await mediator.SendAsync(review, cancellationToken);

            if (review.IsLastAdministrator)
            {
                return;
            }
        }

        var role = await roleQuery.FindAsync(new Query.Role.Specification().AddId(message.RoleId), cancellationToken: cancellationToken)
                   ?? throw new ApplicationException($"Could not find a role with id '{message.RoleId}'.");

        var stream = await eventStore.GetAsync(message.IdentityId, cancellationToken);
        var identity = stream.Get<Identity>();

        if (!identity.IsInTenant(role.TenantId))
        {
            throw new ApplicationException($"Identity '{identity.Name}' is not in tenant with id '{role.TenantId}'.");
        }

        if (message.Active && !identity.IsInRole(message.RoleId))
        {
            stream.Add(identity.AddRole(message.RoleId));
        }

        if (!message.Active && identity.IsInRole(message.RoleId))
        {
            stream.Add(identity.RemoveRole(message.RoleId));
        }

        if (stream.ShouldSave())
        {
            await eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }
    }
}
