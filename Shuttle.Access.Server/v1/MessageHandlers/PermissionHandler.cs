﻿using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class PermissionHandler :
    IMessageHandler<RegisterPermission>,
    IMessageHandler<SetPermissionStatus>,
    IMessageHandler<SetPermissionName>
{
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IMediator _mediator;

    public PermissionHandler(IDatabaseContextFactory databaseContextFactory, IMediator mediator)
    {
        Guard.AgainstNull(databaseContextFactory);
        Guard.AgainstNull(mediator);

        _databaseContextFactory = databaseContextFactory;
        _mediator = mediator;
    }

    public async Task ProcessMessageAsync(IHandlerContext<RegisterPermission> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        var requestResponse = new RequestResponseMessage<RegisterPermission, PermissionRegistered>(message);

        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            await _mediator.SendAsync(requestResponse);
        }

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetPermissionName> context)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetPermissionName, PermissionNameSet>(message);

        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            await _mediator.SendAsync(requestResponse);
        }

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetPermissionStatus> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        var requestResponse = new RequestResponseMessage<SetPermissionStatus, PermissionStatusSet>(message);

        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            await _mediator.SendAsync(requestResponse);
        }

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response);
        }
    }
}