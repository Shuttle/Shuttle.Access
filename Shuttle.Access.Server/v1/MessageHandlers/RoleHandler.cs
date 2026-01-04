using System;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RoleHandler(IMediator mediator) :
    IMessageHandler<RegisterRole>,
    IMessageHandler<RemoveRole>,
    IMessageHandler<SetRolePermission>,
    IMessageHandler<SetRoleName>
{
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);

    public async Task ProcessMessageAsync(IHandlerContext<RegisterRole> context, CancellationToken cancellationToken = default)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var registerRole = new Application.RegisterRole(message.Name);

        registerRole.AddPermissions(message.Permissions);

        var requestResponse = new RequestResponseMessage<Application.RegisterRole, RoleRegistered>(registerRole);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (registerRole.HasMissingPermissions)
        {
            if (message.WaitCount < 5)
            {
                message.WaitCount++;

                await context.SendAsync(message, builder => builder.DeferUntil(DateTime.Now.AddSeconds(5)).ToSelf(), cancellationToken);

                return;
            }

            throw new UnrecoverableHandlerException("Maximum permission wait count reached.");
        }

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<RemoveRole> context, CancellationToken cancellationToken = default)
    {
        var message = context.Message;

        var requestResponse = new RequestResponseMessage<RemoveRole, RoleRemoved>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetRoleName> context, CancellationToken cancellationToken = default)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetRoleName, RoleNameSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetRolePermission> context, CancellationToken cancellationToken = default)
    {
        var message = context.Message;

        var requestResponse = new RequestResponseMessage<SetRolePermission, RolePermissionSet>(message);

        await _mediator.SendAsync(requestResponse, cancellationToken);

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response, cancellationToken: cancellationToken);
        }
    }
}