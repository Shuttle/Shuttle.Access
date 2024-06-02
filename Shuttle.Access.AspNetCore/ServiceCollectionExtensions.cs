using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccessAuthorization(this IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            services.AddSingleton<AccessAuthorizationMiddleware>();

            return services;
        }
    }
}