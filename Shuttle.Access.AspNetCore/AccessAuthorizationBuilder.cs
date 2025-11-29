using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public class AccessAuthorizationBuilder(IServiceCollection services)
{
    private AccessAuthorizationOptions _options = new();

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);

    public AccessAuthorizationOptions Options
    {
        get => _options;
        set => _options = value ?? throw new ArgumentNullException(nameof(value));
    }
}