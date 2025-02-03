using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class AccessClientBuilder
{
    private AccessClientOptions _accessClientOptions = new();

    public AccessClientBuilder(IServiceCollection services)
    {
        Guard.AgainstNull(services);

        Services = services;
    }

    public AccessClientOptions Options
    {
        get => _accessClientOptions;
        set => _accessClientOptions = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IServiceCollection Services { get; }
}