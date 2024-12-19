using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access;

public class AccessBuilder
{
    private AccessOptions _accessOptions = new();

    public AccessBuilder(IServiceCollection services)
    {
        Guard.AgainstNull(services);

        Services = services;
    }

    public AccessOptions Options
    {
        get => _accessOptions;
        set => _accessOptions = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IServiceCollection Services { get; }
}