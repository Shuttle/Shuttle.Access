using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class IdentityHandler(IMediator mediator) :
    IMessageHandler<RegisterIdentity>,
    IMessageHandler<SetIdentityRoleStatus>,
    IMessageHandler<SetIdentityTenantStatus>,
    IMessageHandler<RemoveIdentity>,
    IMessageHandler<SetPassword>,
    IMessageHandler<ActivateIdentity>,
    IMessageHandler<SetIdentityName>,
    IMessageHandler<SetIdentityDescription>
{
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);

    public async Task HandleAsync(ActivateIdentity message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }

    public async Task HandleAsync(RegisterIdentity message, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(message);

        if (string.IsNullOrEmpty(message.Name) ||
            string.IsNullOrEmpty(message.AuditIdentityName) ||
            message.PasswordHash.Length == 0)
        {
            return;
        }

        await _mediator.SendAsync(message, cancellationToken);
    }

    public async Task HandleAsync(RemoveIdentity message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }

    public async Task HandleAsync(SetIdentityDescription message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Description))
        {
            return;
        }

        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }

    public async Task HandleAsync(SetIdentityName message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(message.Name))
        {
            return;
        }

        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }

    public async Task HandleAsync(SetIdentityRoleStatus message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }

    public async Task HandleAsync(SetIdentityTenantStatus message, CancellationToken cancellationToken = default)
    {
         await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }

    public async Task HandleAsync(SetPassword message, CancellationToken cancellationToken = default)
    {
        await _mediator.SendAsync(Guard.AgainstNull(message), cancellationToken);
    }
}