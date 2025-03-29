using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shuttle.Access.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfCoreAccess(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContextFactory<AccessDbContext>(builder =>
        {
            var connectionString = configuration.GetConnectionString("Access");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Could not find a connection string for 'Access'.");
            }

            builder.UseSqlServer(connectionString, sqlServerOptions => { sqlServerOptions.CommandTimeout(300); });
        });

        return services;
    }
}