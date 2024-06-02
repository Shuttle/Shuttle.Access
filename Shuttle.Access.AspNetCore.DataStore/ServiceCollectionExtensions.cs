using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc.DataStore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataStoreAccessService(this IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            services.TryAddSingleton<IAccessService, DataStoreAccessService>();

            return services;
        }
    }
}