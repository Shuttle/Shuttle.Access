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
        public AccessBuilder UseSqlServer(Action<AccessSqlServerOptions>? configureOptions = null)
        {
            var services = accessBuilder.Services;

            services.AddOptions<AccessSqlServerOptions>().Configure(options =>
            {
                configureOptions?.Invoke(options);
            });

            services.AddSingleton<IValidateOptions<AccessSqlServerOptions>, AccessSqlServerOptionsValidator>();

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
                var accessSqlServerOptions = serviceProvider.GetRequiredService<IOptions<AccessSqlServerOptions>>();

                if (dbConnection != null)
                {
                    options.UseSqlServer(dbConnection, Configure);
                }
                else
                {
                    options.UseSqlServer(accessSqlServerOptions.Value.ConnectionString, Configure);
                }

                return;

                void Configure(SqlServerDbContextOptionsBuilder sqlServerOptions)
                {
                    sqlServerOptions.CommandTimeout(accessSqlServerOptions.Value.CommandTimeout.Seconds);
                }
            });

            return accessBuilder;
        }
    }
}