using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;
using Shuttle.Core.DependencyInjection;

namespace Shuttle.Access.Sql
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlAccess(this IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            services.FromAssembly(Assembly.Load("Shuttle.Access.Sql")).Add();

            return services;
        }
    }
}