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

        var registerRole = new Application.RegisterRole( message.Name, new AuditInformation(message.TenantId, message.AuditIdentityName));

        registerRole.AddPermissions(message.Permissions);

        var requestResponse = new RequestResponseMessage<Application.RegisterRole, RoleRegistered>(registerRole);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (registerRole.HasMissingPermissions)
        {
            if (message.WaitCount >= 5)
            {
                throw new UnrecoverableHandlerException("Maximum permission wait count reached.");
            }

            message.WaitCount++;

            await _bus.SendAsync(message, builder => builder.DeferUntil(DateTime.Now.AddSeconds(5)).ToSelf(), cancellationToken);

            return;

        }

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleAsync(RemoveRole message, CancellationToken cancellationToken = default)
    {
        var requestResponse = new RequestResponseMessage<RemoveRole, RoleRemoved>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleAsync(SetRoleName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetRoleName, RoleNameSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task HandleAsync(SetRolePermissionStatus message, CancellationToken cancellationToken = default)
    {
        var requestResponse = new RequestResponseMessage<SetRolePermissionStatus, RolePermissionSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await _bus.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }
}