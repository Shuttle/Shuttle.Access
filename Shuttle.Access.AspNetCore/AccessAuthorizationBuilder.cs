using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
}