using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Mediator;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class MonitorKeepAliveHandler(ILogger<MonitorKeepAliveHandler> logger, IOptions<ServerOptions> serverOptions, IMediator mediator, IBus bus, IKeepAliveContext keepAliveContext)
    : IMessageHandler<MonitorKeepAlive>
{
    private readonly IBus _bus = Guard.AgainstNull(bus);
    private readonly IKeepAliveContext _keepAliveContext = Guard.AgainstNull(keepAliveContext);
    private readonly ILogger<MonitorKeepAliveHandler> _logger = Guard.AgainstNull(logger);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly ServerOptions _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);

    public async Task HandleAsync(MonitorKeepAlive message, CancellationToken cancellationToken = default)
    {
        var result = new Application.MonitorKeepAlive();

        await _mediator.SendAsync(result, cancellationToken);

        if (result.ShouldReset)
        {
            _logger.LogDebug("[keep-alive] : reset");

            await _keepAliveContext.ResetAsync();

            return;
        }

        var ignoreTillDate = DateTime.UtcNow.Add(_serverOptions.MonitorKeepAliveInterval);

        await _bus.SendAsync(new MonitorKeepAlive(), builder =>
        {
            builder.ToSelf().DeferUntil(ignoreTillDate);
        }, cancellationToken);

        _logger.LogDebug("[keep-alive] : ignore till date = '{IgnoreTillDate:O}'", ignoreTillDate);
    }
}
