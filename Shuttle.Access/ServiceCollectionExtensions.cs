using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public AccessBuilder AddAccess(Action<AccessOptions>? configureOptions = null)
        {
            Guard.AgainstNull(services);

            var builder = new AccessBuilder(services);

            services.AddOptions<AccessOptions>().Configure(options =>
            {
                configureOptions?.Invoke(options);
            });

            services.AddSingleton<IValidateOptions<AccessOptions>, AccessOptionsValidator>();

            return builder;
        }
    }
}