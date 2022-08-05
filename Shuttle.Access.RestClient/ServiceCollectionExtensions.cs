using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccessRestClient(this IServiceCollection services,
            Action<AccessRestClientBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var accessRestClientBuilder = new AccessRestClientBuilder(services);

            builder?.Invoke(accessRestClientBuilder);

            services.AddOptions<AccessRestClientOptions>().Configure(options =>
            {
                options.BaseAddress = accessRestClientBuilder.Options.BaseAddress;
                options.IdentityName = accessRestClientBuilder.Options.IdentityName;
                options.Password = accessRestClientBuilder.Options.Password;
            });

            return services;
        }
    }
}