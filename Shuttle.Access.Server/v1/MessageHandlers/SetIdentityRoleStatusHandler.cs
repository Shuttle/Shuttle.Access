using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetIdentityRoleStatusHandler(IEventStore eventStore, IRoleQuery roleQuery, IIdentityQuery identityQuery) : IMessageHandler<SetIdentityRoleStatus>
{
    public async Task HandleAsync(SetIdentityRoleStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (!message.Active)
        {
            var roles = (await roleQuery.SearchAsync(new Query.Role.Specification()
                .WithTenantId(message.AuditTenantId)
                .AddName("Access Administrator"), cancellationToken)).ToList();

            if (roles.Count != 1)
            {
                return;
            }

            var role = roles[0];

            if (message.RoleId.Equals(role.Id) &&
                await identityQuery.AdministratorCountAsync(message.AuditTenantId, cancellationToken) == 1)
            {
                return;
            }
        }

        var identity = new Identity();
        var stream = await eventStore.GetAsync(message.IdentityId, cancellationToken);

        stream.Apply(identity);

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