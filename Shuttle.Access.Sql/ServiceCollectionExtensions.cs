using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Contract;
using Shuttle.Core.DependencyInjection;

namespace Shuttle.Access.Sql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlAccess(this IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            services.TryAddSingleton<IAccessService, DataStoreAccessService>();
            services.FromAssembly(typeof(ServiceCollectionExtensions).GetTypeInfo().Assembly).Add();

            return services;
        }
    }
}