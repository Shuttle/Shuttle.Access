using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Access.Server.v1.MessageHandlers;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Esb;

namespace Shuttle.Access.Server;

public class KeepAliveObserver : IPipelineObserver<OnAfterGetMessage>
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IServiceBus _serviceBus;
    private bool _keepAliveSent;
    private readonly ServerOptions _serverOptions;

    public KeepAliveObserver(IOptions<ServerOptions> serverOptions, IServiceBus serviceBus)
    {
        _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);
        _serviceBus = Guard.AgainstNull(serviceBus);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAfterGetMessage> pipelineContext)
    {
        await _lock.WaitAsync();

        try
        {
            if (_keepAliveSent)
            {
                return;
            }

            await _serviceBus.SendAsync(new MonitorKeepAlive(), builder => builder.Local().Defer(DateTime.UtcNow.Add(_serverOptions.MonitorKeepAliveInterval)));

            _keepAliveSent = true;
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