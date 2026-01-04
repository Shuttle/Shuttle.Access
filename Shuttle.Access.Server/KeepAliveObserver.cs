using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Access.Server.v1.MessageHandlers;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Hopper;

namespace Shuttle.Access.Server;

public class KeepAliveObserver(ILogger<KeepAliveObserver> logger, IOptions<ServerOptions> serverOptions, IServiceBus serviceBus)
    : IPipelineObserver<MessageReceived>
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger<KeepAliveObserver> _logger = Guard.AgainstNull(logger);
    private readonly ServerOptions _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);
    private readonly IServiceBus _serviceBus = Guard.AgainstNull(serviceBus);
    private bool _keepAliveSent;

    public async Task ExecuteAsync(IPipelineContext<MessageReceived> pipelineContext, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_keepAliveSent)
            {
                return;
            }

            var ignoreTillDate = DateTime.UtcNow.Add(_serverOptions.MonitorKeepAliveInterval);

            await _serviceBus.SendAsync(new MonitorKeepAlive(), builder =>
            {
                builder.ToSelf().DeferUntil(ignoreTillDate);
            }, cancellationToken);

            _keepAliveSent = true;

            _logger.LogDebug($"[keep-alive] : ignore till date = '{ignoreTillDate:O}'");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ResetAsync()
    {
        await _lock.WaitAsync();

        try
        {
            _keepAliveSent = false;
        }
        finally
        {
            _lock.Release();
        }
    }
}