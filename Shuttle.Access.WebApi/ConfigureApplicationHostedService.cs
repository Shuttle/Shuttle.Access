using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Contract;
using Shuttle.Mediator;

namespace Shuttle.Access.WebApi;

public class ConfigureApplicationHostedService(IOptions<ApiOptions> apiOptions, IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    private readonly ApiOptions _apiOptions = Guard.AgainstNull(Guard.AgainstNull(apiOptions).Value);
    private readonly IServiceScopeFactory _serviceScopeFactory = Guard.AgainstNull(serviceScopeFactory);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IMediator>().SendAsync(new ConfigureApplication(_apiOptions.ConfigureApplicationTimeout), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
