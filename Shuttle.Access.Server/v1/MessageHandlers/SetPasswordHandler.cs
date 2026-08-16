using Shuttle.Access.Messages.v1;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class SetPasswordHandler(IMediator mediator) : IMessageHandler<SetPassword>
{
    public async Task HandleAsync(SetPassword message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        await mediator.SendAsync(new Application.SetPassword(message.Id, message.PasswordHash), cancellationToken);
    }
}
