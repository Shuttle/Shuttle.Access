using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Mediator;
using Shuttle.Hopper;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.Storage;
using RoleRegistered = Shuttle.Access.Messages.v2.RoleRegistered;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RegisterRoleHandler(IBus bus, IMediator mediator, IPermissionQuery permissionQuery)
    : IMessageHandler<RegisterRole>
{
    public async Task HandleAsync(RegisterRole message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(mediator);
        ArgumentNullException.ThrowIfNull(permissionQuery);

        var permissionIds = new List<Guid>();

        foreach (var permission in message.Permissions)
        {
            var permissionId = (await permissionQuery.SearchAsync(new Query.Permission.Specification().AddName(permission.Name), cancellationToken)).FirstOrDefault()?.Id;

            if (permissionId.HasValue)
            {
                permissionIds.Add(permissionId.Value);
            }
            else
            {
                if (message.WaitCount >= 5)
                {
                    throw new UnrecoverableHandlerException("Maximum permission wait count reached.");
                }

                message.WaitCount++;

                await bus.SendAsync(message, builder => builder.DeferUntil(DateTime.Now.AddSeconds(5)).ToSelf(), cancellationToken);
                return;
            }
        }

        var registerRole = new Application.RegisterRole(message.Id, message.TenantId, message.Name, message.AuditTenantId, message.AuditIdentityName);

        foreach (var permissionId in permissionIds)
        {
            registerRole.AddPermissionId(permissionId);
        }

        await mediator.SendAsync(registerRole, cancellationToken);
    }
}