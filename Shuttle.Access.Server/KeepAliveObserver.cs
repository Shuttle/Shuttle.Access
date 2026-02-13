using Microsoft.Extensions.Logging;
using Shuttle.Access.Server.v1.MessageHandlers;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Hopper;

namespace Shuttle.Access.Server;

public class KeepAliveObserver(ILogger<KeepAliveObserver> logger, IBus bus, IKeepAliveContext keepAliveContext)
    : IPipelineObserver<MessageReceived>
{
    private readonly IKeepAliveContext _keepAliveContext = Guard.AgainstNull(keepAliveContext);
    private readonly ILogger<KeepAliveObserver> _logger = Guard.AgainstNull(logger);
    private readonly IBus _bus = Guard.AgainstNull(bus);

    public async Task ExecuteAsync(IPipelineContext<MessageReceived> pipelineContext, CancellationToken cancellationToken = default)
    {
        if (await _keepAliveContext.GetShouldIgnoreAsync())
        {
            return;
        }

        var ignoreTillDate = _keepAliveContext.GetIgnoreTillDate();

        await _bus.SendAsync(new MonitorKeepAlive(), builder =>
        {
            builder.ToSelf().DeferUntil(ignoreTillDate);
        }, cancellationToken);

        await _keepAliveContext.SentAsync();

        _logger.LogDebug("[keep-alive] : ignore till date = '{IgnoreTillDate:O}'", ignoreTillDate);
    }
}