using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.Access;

public class AccessBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
}