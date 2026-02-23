using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RoleHandler(IBus bus, IMediator mediator) :
    IMessageHandler<RegisterRole>,
    IMessageHandler<RemoveRole>,
    IMessageHandler<SetRolePermissionStatus>,
    IMessageHandler<SetRoleName>
{
    private readonly IBus _bus = Guard.AgainstNull(bus);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);

    public async Task HandleAsync(RegisterRole message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var registerRole = new Application.RegisterRole(Guid.NewGuid(), message.Name, message.TenantId, new AuditInformation(message.TenantId, message.AuditIdentityName));

        registerRole.AddPermissions(message.Permissions);

        await _mediator.SendAsync(registerRole, cancellationToken);

        if (registerRole.HasMissingPermissions)
        {
            if (message.WaitCount >= 5)
            {
                throw new UnrecoverableHandlerException("Maximum permission wait count reached.");
            }

            message.WaitCount++;

            await _bus.SendAsync(message, builder => builder.DeferUntil(DateTime.Now.AddSeconds(5)).ToSelf(), cancellationToken);
        }
    }

    public async Task HandleAsync(RemoveRole message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }

    public async Task HandleAsync(SetRoleName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(Guard.AgainstNull(message).Name))
        {
            return;
        }

        await _mediator.SendAsync(message, cancellationToken);
    }

    public async Task HandleAsync(SetRolePermissionStatus message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }
}