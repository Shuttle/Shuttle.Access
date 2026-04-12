using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public class AccessClientBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
}