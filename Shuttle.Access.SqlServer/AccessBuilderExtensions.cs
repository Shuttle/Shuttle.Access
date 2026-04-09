using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Shuttle.Access.SqlServer;

public static class AccessBuilderExtensions
{
    extension(AccessBuilder accessBuilder)
    {
        public IServiceCollection UseSqlServer(Action<AccessSqlServerBuilder>? builder = null)
        {
            var services = accessBuilder.Services;
            var accessSqlServerBuilder = new AccessSqlServerBuilder(services);

            builder?.Invoke(accessSqlServerBuilder);

            services.AddSingleton<IValidateOptions<AccessSqlServerOptions>, AccessSqlServerOptionsValidator>();

            services.AddOptions<AccessSqlServerOptions>().Configure(options =>
            {
                options.ConnectionString = accessSqlServerBuilder.Options.ConnectionString;
                options.CommandTimeout = accessSqlServerBuilder.Options.CommandTimeout;
            });

            services.TryAddSingleton<ISessionCache, SessionCache>();

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IIdentityQuery, IdentityQuery>();
            services.AddScoped<IPermissionQuery, PermissionQuery>();
            services.AddScoped<IRoleQuery, RoleQuery>();
            services.AddScoped<ISessionQuery, SessionQuery>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<ITenantQuery, TenantQuery>();

            services.AddKeyedScoped<DbConnection>("AccessDbConnection", (serviceProvider, _) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<AccessSqlServerOptions>>().Value;
                return new SqlConnection(options.ConnectionString);
            });
            
            services.AddDbContext<AccessDbContext>((serviceProvider, options) =>
            {
                var dbConnection = serviceProvider.GetKeyedService<DbConnection>("AccessDbConnection");

                if (dbConnection != null)
                {
                    options.UseSqlServer(dbConnection, Configure);
                }
                else
                {
                    options.UseSqlServer(accessSqlServerBuilder.Options.ConnectionString, Configure);
                }
            });

            return services;

            void Configure(SqlServerDbContextOptionsBuilder sqlServerOptions)
            {
                sqlServerOptions.CommandTimeout(accessSqlServerBuilder.Options.CommandTimeout.Seconds);
            }
        }
    }
}