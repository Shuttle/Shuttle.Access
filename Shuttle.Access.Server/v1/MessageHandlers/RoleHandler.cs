﻿using System;
using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class RoleHandler :
    IMessageHandler<RegisterRole>,
    IMessageHandler<RemoveRole>,
    IMessageHandler<SetRolePermission>,
    IMessageHandler<SetRoleName>
{
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IMediator _mediator;

    public RoleHandler(IDatabaseContextFactory databaseContextFactory, IMediator mediator)
    {
        Guard.AgainstNull(databaseContextFactory);
        Guard.AgainstNull(mediator);

        _databaseContextFactory = databaseContextFactory;
        _mediator = mediator;
    }

    public async Task ProcessMessageAsync(IHandlerContext<RegisterRole> context)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var registerRole = new Application.RegisterRole(message.Name);

        registerRole.AddPermissions(message.Permissions);

        var requestResponse = new RequestResponseMessage<Application.RegisterRole, RoleRegistered>(registerRole);

        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            await _mediator.SendAsync(requestResponse);
        }

        if (registerRole.HasMissingPermissions)
        {
            if (message.WaitCount < 5)
            {
                message.WaitCount++;

                await context.SendAsync(message, builder => builder.Defer(DateTime.Now.AddSeconds(5)).Local());

                return;
            }

            throw new UnrecoverableHandlerException("Maximum permission wait count reached.");
        }

        if (requestResponse.Response != null)
        {
            await context.PublishAsync(requestResponse.Response);
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<RemoveRole> context)
    {
        var message = context.Message;

        var requestResponse = new RequestResponseMessage<RemoveRole, RoleRemoved>(message);

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

    public async Task ProcessMessageAsync(IHandlerContext<SetRoleName> context)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetRoleName, RoleNameSet>(message);

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

    public async Task ProcessMessageAsync(IHandlerContext<SetRolePermission> context)
    {
        var message = context.Message;

        var requestResponse = new RequestResponseMessage<SetRolePermission, RolePermissionSet>(message);

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