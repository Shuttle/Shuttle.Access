using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public class SetIdentityRoleStatusParticipant(IEventStore eventStore, IRoleQuery roleQuery, IIdentityQuery identityQuery) : IParticipant<SetIdentityRoleStatus>
{
    private readonly IRoleQuery _roleQuery = Guard.AgainstNull(roleQuery);
    private readonly IIdentityQuery _identityQuery = Guard.AgainstNull(identityQuery);
    private readonly IEventStore _eventStore = Guard.AgainstNull(eventStore);

    public async Task HandleAsync(SetIdentityRoleStatus message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (!message.Active)
        {
            var roles = (await _roleQuery.SearchAsync(new RoleSpecification()
                .WithTenantId(message.AuditTenantId)
                .AddName("Access Administrator"), cancellationToken)).ToList();

            if (roles.Count != 1)
            {
                return;
            }

            var role = roles[0];

            if (message.RoleId.Equals(role.Id) &&
                await _identityQuery.AdministratorCountAsync(message.AuditTenantId, cancellationToken) == 1)
            {
                return;
            }
        }

        var identity = new Identity();
        var stream = await _eventStore.GetAsync(message.IdentityId, cancellationToken);

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
            await _eventStore.SaveAsync(stream, builder => builder.Audit(message), cancellationToken);
        }
    }
}