using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers a client that calls the Shuttle.Access web API using *this* application's own identity.  An
        ///     authentication provider has to be registered on the returned builder — either
        ///     `UseBearerAuthenticationProvider(...)` or `UsePasswordAuthenticationProvider(...)` — since there is no
        ///     other credential available to the client.
        /// </summary>
        public AccessClientBuilder AddAccessClient(Action<AccessClientOptions>? configureOptions = null)
        {
            Guard.AgainstNull(services);

            var builder = new AccessClientBuilder(services);

            services.AddOptions<AccessClientOptions>().Configure(options =>
            {
                configureOptions?.Invoke(options);
            });

            services.AddSingleton<IValidateOptions<AccessClientOptions>, AccessClientOptionsValidator>();
            services.AddHostedService<AuthenticationInterceptorStartupValidator>();

            services.AddTransient<AccessHttpMessageHandler>();
            services.AddHttpClient<IAccessClient, AccessClient>(AccessClient.HttpClientName, (serviceProvider, client) =>
                {
                    client.BaseAddress = new(serviceProvider.GetRequiredService<IOptions<AccessClientOptions>>().Value.BaseAddress);
                })
                .AddHttpMessageHandler<AccessHttpMessageHandler>();

            return builder;
        }
    }
}
