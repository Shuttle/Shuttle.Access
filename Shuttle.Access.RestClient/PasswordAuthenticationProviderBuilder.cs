using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public class PasswordAuthenticationProviderBuilder
{
    private PasswordAuthenticationProviderOptions _options = new();

    public PasswordAuthenticationProviderBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public PasswordAuthenticationProviderOptions Options
    {
        get => _options;
        set => _options = value ?? throw new ArgumentNullException(nameof(value));
    }

    public IServiceCollection Services { get; }
}