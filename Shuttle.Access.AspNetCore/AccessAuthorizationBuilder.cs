using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationBuilder
{
    private AccessAuthorizationOptions _options = new();

    public AccessAuthorizationBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public IServiceCollection Services { get; }

    public AccessAuthorizationOptions Options
    {
        get => _options;
        set => _options = value ?? throw new ArgumentNullException(nameof(value));
    }
}