using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Core.Pipelines;
using Shuttle.Hopper;

namespace Shuttle.Access.Server;

public class ServerHostedService(IOptions<PipelineOptions> pipelineOptions, IOptions<ServerOptions> serverOptions, IMediator mediator)
    : BackgroundService
{
    private readonly Type _inboxMessagePipeline = typeof(InboxMessagePipeline);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly PipelineOptions _pipelineOptions = Guard.AgainstNull(Guard.AgainstNull(pipelineOptions).Value);
    private readonly ServerOptions _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _mediator.SendAsync(new ConfigureApplication(), cancellationToken);
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

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineCreated -= PipelineCreated;

        return Task.CompletedTask;
    }
}