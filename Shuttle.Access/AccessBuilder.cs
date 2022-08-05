using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public class AccessBuilder
    {
    public AccessOptions Options
    {
        get => _accessOptions;
        set => _accessOptions = value ?? throw new ArgumentNullException(nameof(value));
    }

    private AccessOptions _accessOptions = new AccessOptions();

    public AccessBuilder(IServiceCollection services)
    {
        Guard.AgainstNull(services, nameof(services));

        Services = services;
    }

    public IServiceCollection Services { get; }
}
}