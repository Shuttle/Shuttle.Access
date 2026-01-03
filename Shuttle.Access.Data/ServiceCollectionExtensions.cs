using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Shuttle.Access.Data;

public static class ServiceCollectionExtensions
{
    extension(AccessBuilder accessBuilder)
    {
        public IServiceCollection UseSqlServer(Action<AccessDataBuilder>? builder = null)
        {
            var services = accessBuilder.Services;
            var accessDataBuilder = new AccessDataBuilder(services);

            builder?.Invoke(accessDataBuilder);

            services.AddScoped<ISessionService, SqlServerSessionService>();

            services.AddDbContextFactory<AccessDbContext>(dbContextFactoryBuilder =>
            {
                dbContextFactoryBuilder.UseSqlServer(accessDataBuilder.Options.ConnectionString, sqlServerOptions =>
                {
                    sqlServerOptions.CommandTimeout(300);
                });
            });

            return services;
        }
    }
}