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
    : IHostedService
{
    private readonly Type _inboxMessagePipeline = typeof(InboxMessagePipeline);
    private readonly PipelineOptions _pipelineOptions = Guard.AgainstNull(Guard.AgainstNull(pipelineOptions).Value);
    private readonly ServerOptions _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineCreated += PipelineCreated;

        using var scope = Guard.AgainstNull(serviceScopeFactory).CreateScope();

        await scope.ServiceProvider.GetRequiredService<IMediator>().SendAsync(new ConfigureApplication(), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineCreated -= PipelineCreated;

        return Task.CompletedTask;
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
}