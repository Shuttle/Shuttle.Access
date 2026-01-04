using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAccessClient(Action<AccessClientBuilder>? builder = null)
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

            services.AddSingleton<RestSessionService>();
            services.AddSingleton<ISessionService>(sp => sp.GetRequiredService<RestSessionService>());
            services.AddSingleton<IContextSessionService>(sp => sp.GetRequiredService<RestSessionService>());

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services;
        }
    }

    [Obsolete("Replace with `UseBearerAuthenticationProvider`.")]
    public static AccessClientBuilder AddBearerAuthenticationProvider(this AccessClientBuilder accessClientBuilder, Action<BearerAuthenticationProviderBuilder>? builder = null)
    {
        return accessClientBuilder.UseBearerAuthenticationProvider(builder);
    }

    [Obsolete("Replace with `UsePasswordAuthenticationProvider`.")]
    public static AccessClientBuilder AddPasswordAuthenticationProvider(this AccessClientBuilder accessClientBuilder, Action<PasswordAuthenticationProviderBuilder>? builder = null)
    {
        return accessClientBuilder.UsePasswordAuthenticationProvider(builder);
    }

    public static AccessClientBuilder UseBearerAuthenticationProvider(this AccessClientBuilder accessClientBuilder, Action<BearerAuthenticationProviderBuilder>? builder = null)
    {
        Guard.AgainstNull(accessClientBuilder);

        accessClientBuilder.Services.TryAddSingleton<IJwtService, JwtService>();
        accessClientBuilder.Services.AddHttpClient<IAuthenticationProvider, BearerAuthenticationProvider>("BearerAuthenticationProvider");

        var bearerAuthenticationProviderBuilder = new BearerAuthenticationProviderBuilder(accessClientBuilder.Services);

        builder?.Invoke(bearerAuthenticationProviderBuilder);

        bearerAuthenticationProviderBuilder.Services
            .Configure<BearerAuthenticationProviderOptions>(options =>
            {
                options.GetTokenAsync = bearerAuthenticationProviderBuilder.Options.GetTokenAsync;
            });

        accessClientBuilder.Options.ConfigureHttpRequestAsync = async (httpRequestMessage, serviceProvider) =>
        {
            httpRequestMessage.Headers.Authorization = await serviceProvider.GetRequiredService<IAuthenticationProvider>().GetAuthenticationHeaderAsync(httpRequestMessage);
        };

        return accessClientBuilder;
    }

    public static AccessClientBuilder UsePasswordAuthenticationProvider(this AccessClientBuilder accessClientBuilder, Action<PasswordAuthenticationProviderBuilder>? builder = null)
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

        accessClientBuilder.Options.ConfigureHttpRequestAsync = async (httpRequestMessage, serviceProvider) =>
        {
            httpRequestMessage.Headers.Authorization = await serviceProvider.GetRequiredService<IAuthenticationProvider>().GetAuthenticationHeaderAsync(httpRequestMessage);
        };

        return accessClientBuilder;
    }
}