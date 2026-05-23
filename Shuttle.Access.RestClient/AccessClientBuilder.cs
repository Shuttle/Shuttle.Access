using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public class AccessClientBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
    public OptionsBuilder<AccessClientOptions> Options => Services.AddOptions<AccessClientOptions>();
}