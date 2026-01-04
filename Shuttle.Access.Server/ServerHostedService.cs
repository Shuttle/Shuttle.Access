using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Core.Pipelines;
using Shuttle.Hopper;

namespace Shuttle.Access.Server;

public class ServerHostedService(IOptions<PipelineOptions> pipelineOptions, IOptions<ServerOptions> serverOptions, IServiceScopeFactory serviceScopeFactory)
    : BackgroundService
{
    private readonly Type _inboxMessagePipeline = typeof(InboxMessagePipeline);
    private IMediator? _mediator;
    private readonly PipelineOptions _pipelineOptions = Guard.AgainstNull(Guard.AgainstNull(pipelineOptions).Value);
    private readonly ServerOptions _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);
    private IServiceScope? _serviceScope;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (_mediator != null)
        {
            await _mediator.SendAsync(new ConfigureApplication(), cancellationToken);
        }
    }

    private Task PipelineCreated(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (!_serverOptions.MonitorKeepAliveInterval.Equals(TimeSpan.Zero) &&
            eventArgs.Pipeline.GetType() == _inboxMessagePipeline)
        {
            eventArgs.Pipeline.AddObserver<KeepAliveObserver>();
        }

        return Task.CompletedTask;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineCreated += PipelineCreated;

        _serviceScope = Guard.AgainstNull(serviceScopeFactory).CreateScope();

        _mediator = _serviceScope.ServiceProvider.GetRequiredService<IMediator>();
        
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineCreated -= PipelineCreated;

        _serviceScope?.Dispose();

        return Task.CompletedTask;
    }
}