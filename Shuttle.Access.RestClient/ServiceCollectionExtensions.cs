using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public AccessClientBuilder AddAccessClient(Action<AccessClientOptions>? configureOptions = null)
        {
            Guard.AgainstNull(services);

            var builder = new AccessClientBuilder(services);

            services.AddOptions<AccessClientOptions>().Configure(options =>
            {
                configureOptions?.Invoke(options);
            });
            
            services.AddSingleton<IValidateOptions<AccessClientOptions>, AccessClientOptionsValidator>();

            services.AddTransient<AccessHttpMessageHandler>();
            services.AddHttpClient<IAccessClient, AccessClient>("AccessClient", (serviceProvider, client) =>
                {
                    client.BaseAddress = new(serviceProvider.GetRequiredService<IOptions<AccessClientOptions>>().Value.BaseAddress);
                })
                .AddHttpMessageHandler<AccessHttpMessageHandler>();

            services.TryAddSingleton<IHashingService, HashingService>();
            services.TryAddSingleton<ISessionCache, SessionCache>();
            
            services
                .AddSingleton<RestSessionService>()
                .AddSingleton<ISessionService>(sp => sp.GetRequiredService<RestSessionService>())
                .AddSingleton<IContextSessionService>(sp => sp.GetRequiredService<RestSessionService>());

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return builder;
        }
    }
}