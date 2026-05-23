using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = Guard.AgainstNull(services);
    public OptionsBuilder<AccessAuthorizationOptions> Options => Services.AddOptions<AccessAuthorizationOptions>();
}