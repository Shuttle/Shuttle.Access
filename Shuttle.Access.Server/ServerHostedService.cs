using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Pipelines;

namespace Shuttle.Access.Server;

public class ServerHostedService(IOptions<PipelineOptions> pipelineOptions, IOptions<ServerOptions> serverOptions, IServiceScopeFactory serviceScopeFactory)
    : IHostedService
{
    private readonly Type _inboxMessagePipeline = typeof(InboxMessagePipeline);
    private readonly PipelineOptions _pipelineOptions = Guard.AgainstNull(Guard.AgainstNull(pipelineOptions).Value);
    private readonly ServerOptions _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineStarting += PipelineStarting;

        using var scope = Guard.AgainstNull(serviceScopeFactory).CreateScope();

        await scope.ServiceProvider.GetRequiredService<IBus>().SendAsync(new ConfigureApplication(), builder => builder.ToSelf(), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _pipelineOptions.PipelineStarting -= PipelineStarting;

        return Task.CompletedTask;
    }

    private Task PipelineStarting(PipelineEventArgs eventArgs, CancellationToken cancellationToken)
    {
        if (!_serverOptions.MonitorKeepAliveInterval.Equals(TimeSpan.Zero) &&
            eventArgs.Pipeline.GetType() == _inboxMessagePipeline)
        {
            eventArgs.Pipeline.AddObserver<KeepAliveObserver>();
        }

        return Task.CompletedTask;
    }
}