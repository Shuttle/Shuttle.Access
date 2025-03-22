using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessClient(this IServiceCollection services, Action<AccessClientBuilder>? builder = null)
    {
        Guard.AgainstNull(services);

        var accessClientBuilder = new AccessClientBuilder(services);

        builder?.Invoke(accessClientBuilder);

        services.Configure<AccessClientOptions>(options =>
        {
            options.BaseAddress = accessClientBuilder.Options.BaseAddress;
            options.RenewToleranceTimeSpan = accessClientBuilder.Options.RenewToleranceTimeSpan;
            options.ConfigureHttpRequestAsync = accessClientBuilder.Options.ConfigureHttpRequestAsync;
        });

        services.AddSingleton<IValidateOptions<AccessClientOptions>, AccessClientOptionsValidator>();

        services.AddTransient<AccessHttpMessageHandler>();
        services.AddHttpClient<IAccessClient, AccessClient>("AccessClient", (serviceProvider, client) =>
            {
                client.BaseAddress = new(serviceProvider.GetRequiredService<IOptions<AccessClientOptions>>().Value.BaseAddress);
            })
            .AddHttpMessageHandler<AccessHttpMessageHandler>();

        services.TryAddSingleton<ISessionCache, RestSessionCache>();

        return services;
    }

    public static AccessClientBuilder AddPasswordAuthenticationProvider(this AccessClientBuilder accessClientBuilder, Action<PasswordAuthenticationProviderBuilder>? builder = null)
    {
        Guard.AgainstNull(accessClientBuilder).Services
            .AddHttpClient<IAuthenticationProvider, PasswordAuthenticationProvider>("PasswordAuthenticationProvider");

        var passwordAuthenticationProviderBuilder = new PasswordAuthenticationProviderBuilder(accessClientBuilder.Services);

        builder?.Invoke(passwordAuthenticationProviderBuilder);

        passwordAuthenticationProviderBuilder.Services
            .Configure<PasswordAuthenticationProviderOptions>(options =>
        {
            options.IdentityName = passwordAuthenticationProviderBuilder.Options.IdentityName;
            options.Password = passwordAuthenticationProviderBuilder.Options.Password;
        });

        passwordAuthenticationProviderBuilder.Services.AddSingleton<IValidateOptions<PasswordAuthenticationProviderOptions>, PasswordAuthenticationProviderOptionsValidator>();

        accessClientBuilder.Options.ConfigureHttpRequestAsync = async (request, serviceProvider) =>
        {
            request.Headers.Authorization = await serviceProvider.GetRequiredService<IAuthenticationProvider>().GetAuthenticationHeaderAsync();
        };

        return accessClientBuilder;
    }
}