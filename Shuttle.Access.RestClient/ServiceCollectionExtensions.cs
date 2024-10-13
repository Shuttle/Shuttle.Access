using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessClient(this IServiceCollection services, Action<AccessClientBuilder> builder = null)
    {
        Guard.AgainstNull(services, nameof(services));

        var accessRestClientBuilder = new AccessClientBuilder(services);

        builder?.Invoke(accessRestClientBuilder);

        services.AddOptions<AccessClientOptions>().Configure(options =>
        {
            options.BaseAddress = accessRestClientBuilder.Options.BaseAddress;
            options.IdentityName = accessRestClientBuilder.Options.IdentityName;
            options.Password = accessRestClientBuilder.Options.Password;
        });

        services.AddTransient<AuthenticationHeaderHandler>();
        services.AddHttpClient<IAccessClient, AccessClient>("AccessClient", (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AccessClientOptions>>().Value;

                client.BaseAddress = options.BaseAddress;
            })
            .AddHttpMessageHandler<AuthenticationHeaderHandler>();

        services.TryAddSingleton<IAccessService, RestAccessService>();

        return services;
    }
}