using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Core.Pipelines;
using Shuttle.Esb;

namespace Shuttle.Access.Server;

public class ServerHostedService : IHostedService
{
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly Type _inboxMessagePipeline = typeof(InboxMessagePipeline);
    private readonly IMediator _mediator;
    private readonly IPipelineFactory _pipelineFactory;
    private readonly ServerOptions _serverOptions;

    public ServerHostedService(IOptions<ServerOptions> serverOptions, IHostApplicationLifetime hostApplicationLifetime, IDatabaseContextFactory databaseContextFactory, IMediator mediator, IPipelineFactory pipelineFactory)
    {
        Guard.AgainstNull(hostApplicationLifetime).ApplicationStarted.Register(OnStarted);

        _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _mediator = Guard.AgainstNull(mediator);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);

        _pipelineFactory.PipelineCreated += OnPipelineCreated;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _pipelineFactory.PipelineCreated -= OnPipelineCreated;

        await Task.CompletedTask;
    }

    private void OnPipelineCreated(object? sender, PipelineEventArgs e)
    {
        if (_serverOptions.MonitorKeepAliveInterval.Equals(TimeSpan.Zero))
        {
            return;
        }

        var pipelineType = e.Pipeline.GetType();

        if (pipelineType == _inboxMessagePipeline)
        {
            e.Pipeline.AddObserver<KeepAliveObserver>();
        }
    }

    private void OnStarted()
    {
        using (_databaseContextFactory.Create())
        {
            _mediator.SendAsync(new ConfigureApplication()).GetAwaiter().GetResult();
        }
    }
}