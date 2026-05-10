using Shuttle.Access.Messages.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetIdentityRoleStatusHandler(IEventStore eventStore, IRoleQuery roleQuery, IIdentityQuery identityQuery) : IMessageHandler<SetIdentityRoleStatus>
{
    public async Task HandleAsync(SetIdentityRoleStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        Query.Role role;

        if (!message.Active)
        {
            var roles = (await roleQuery.SearchAsync(new Query.Role.Specification()
                .WithTenantId(message.AuditTenantId)
                .AddName("Access Administrator"), cancellationToken)).ToList();

            if (roles.Count != 1)
            {
                return;
            }

            role = roles[0];

            if (message.RoleId.Equals(role.Id) &&
                await identityQuery.AdministratorCountAsync(message.AuditTenantId, cancellationToken) == 1)
            {
                return;
            }
        }

        role = (await roleQuery.FindAsync(new Query.Role.Specification().AddId(message.RoleId), cancellationToken: cancellationToken)).GuardAgainstRecordNotFound(message.RoleId);

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