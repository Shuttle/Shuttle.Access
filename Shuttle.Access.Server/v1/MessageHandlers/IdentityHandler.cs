﻿using System.Threading.Tasks;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class IdentityHandler :
    IMessageHandler<RegisterIdentity>,
    IMessageHandler<SetIdentityRole>,
    IMessageHandler<RemoveIdentity>,
    IMessageHandler<SetPassword>,
    IMessageHandler<ActivateIdentity>,
    IMessageHandler<SetIdentityName>,
    IMessageHandler<SetIdentityDescription>
{
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IMediator _mediator;

    public IdentityHandler(IDatabaseContextFactory databaseContextFactory, IMediator mediator)
    {
        Guard.AgainstNull(databaseContextFactory);
        Guard.AgainstNull(mediator);

        _databaseContextFactory = databaseContextFactory;
        _mediator = mediator;
    }

    public async Task ProcessMessageAsync(IHandlerContext<ActivateIdentity> context)
    {
        Guard.AgainstNull(context);

        var requestResponse = new RequestResponseMessage<ActivateIdentity, IdentityActivated>(context.Message);

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

    public async Task ProcessMessageAsync(IHandlerContext<RegisterIdentity> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name) ||
            string.IsNullOrEmpty(message.RegisteredBy) ||
            message.PasswordHash.Length == 0)
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(message);

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

    public async Task ProcessMessageAsync(IHandlerContext<RemoveIdentity> context)
    {
        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            await _mediator.SendAsync(context.Message);

            await context.PublishAsync(new IdentityRemoved
            {
                Id = context.Message.Id
            });
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetIdentityName> context)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetIdentityName, IdentityNameSet>(message);

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

    public async Task ProcessMessageAsync(IHandlerContext<SetIdentityDescription> context)
    {
        var message = context.Message;

        if (string.IsNullOrEmpty(message.Description))
        {
            return;
        }

        var requestResponse = new RequestResponseMessage<SetIdentityDescription, IdentityDescriptionSet>(message);

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

    public async Task ProcessMessageAsync(IHandlerContext<SetIdentityRole> context)
    {
        Guard.AgainstNull(context);

        var message = context.Message;
        var reviewRequest = new RequestMessage<SetIdentityRole>(message);
        var requestResponse = new RequestResponseMessage<SetIdentityRole, IdentityRoleSet>(message);

        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            await _mediator.SendAsync(reviewRequest);

            if (!reviewRequest.Ok)
            {
                return;
            }

            await _mediator.SendAsync(requestResponse);

            if (requestResponse.Response != null)
            {
                await context.PublishAsync(requestResponse.Response);
            }
        }
    }

    public async Task ProcessMessageAsync(IHandlerContext<SetPassword> context)
    {
        Guard.AgainstNull(context);

        using (new DatabaseContextScope())
        await using (_databaseContextFactory.Create())
        {
            await _mediator.SendAsync(context.Message);
        }
    }
}