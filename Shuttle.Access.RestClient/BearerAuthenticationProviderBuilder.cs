using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class BearerAuthenticationProviderBuilder
{
    private BearerAuthenticationProviderOptions _options = new();

    public BearerAuthenticationProviderBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public BearerAuthenticationProviderOptions Options
    {
        get => _options;
        set => _options = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IServiceCollection Services { get; }
}