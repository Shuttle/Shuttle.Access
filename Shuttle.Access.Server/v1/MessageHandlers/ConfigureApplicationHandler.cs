using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class ConfigureApplicationHandler(ILogger<ConfigureApplicationHandler> logger, IOptions<ServerOptions> serverOptions, IMediator mediator, IBus bus)
    : IContextMessageHandler<ConfigureApplication>
{
    public async Task HandleAsync(IHandlerContext<ConfigureApplication> context, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(context);

        var transportMessage = Guard.AgainstNull(context.State.GetTransportMessage());

        if (transportMessage.SentAt < (DateTimeOffset.UtcNow - TimeSpan.FromSeconds(30)))
        {
            logger.LogWarning($"Message 'ConfigureApplication' was sent at '{transportMessage.SentAt}'.  The message is too old and has been ignored.  Another message may have been sent at server startup; else re-start the server.");
            return;
        }

        var configureApplication = new Application.ConfigureApplication(serverOptions.Value.Timeout);

        await mediator.SendAsync(configureApplication, cancellationToken);

        if (configureApplication.ShouldRetry)
        {
            await bus.SendAsync(context.Message, builder => builder.ToSelf().DeferFor(TimeSpan.FromSeconds(5)), cancellationToken);
        }
    }
}
