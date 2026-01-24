using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace Shuttle.Access.SqlServer;

public static class ServiceCollectionExtensions
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

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IIdentityQuery, IdentityQuery>();
            services.AddScoped<IPermissionQuery, PermissionQuery>();
            services.AddScoped<IRoleQuery, RoleQuery>();
            services.AddScoped<ISessionQuery, SessionQuery>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<ISessionTokenExchangeRepository, SessionTokenExchangeRepository>();
            services.AddScoped<ITenantQuery, TenantQuery>();
            services.AddScoped<ITenantProjectionQuery, TenantProjectionQuery>();

            services.AddScoped<DbConnection>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<AccessSqlServerOptions>>().Value;
                return new SqlConnection(options.ConnectionString);
            });
            
            services.AddDbContext<AccessDbContext>((sp, options) =>
            {
                var dbConnection = sp.GetService<DbConnection>();

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