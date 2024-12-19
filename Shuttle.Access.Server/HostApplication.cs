using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Server;

public class ApplicationHostedService: IHostedService
{
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IMediator _mediator;

    public ApplicationHostedService(IHostApplicationLifetime hostApplicationLifetime, IDatabaseContextFactory databaseContextFactory, IMediator mediator)
    {
        Guard.AgainstNull(hostApplicationLifetime, nameof(hostApplicationLifetime)).ApplicationStarted.Register(OnStarted);

        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _mediator = Guard.AgainstNull(mediator);
    }

    private void OnStarted()
    {
        using (_databaseContextFactory.Create())
        {
            _mediator.SendAsync(new ConfigureApplication()).GetAwaiter().GetResult();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}