using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Shuttle.Access.SqlServer;

public static class ServiceCollectionExtensions
{
    extension(AccessBuilder accessBuilder)
    {
        public IServiceCollection UseSqlServer(Action<AccessSqlServerBuilder>? builder = null)
        {
            var services = accessBuilder.Services;
            var accessDataBuilder = new AccessSqlServerBuilder(services);

            builder?.Invoke(accessDataBuilder);

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IIdentityProjectionQuery, IdentityProjectionQuery>();
            services.AddScoped<IIdentityQuery, IdentityQuery>();
            services.AddScoped<IPermissionProjectionQuery, PermissionProjectionQuery>();
            services.AddScoped<IPermissionQuery, PermissionQuery>();
            services.AddScoped<IRoleProjectionQuery, RoleProjectionQuery>();
            services.AddScoped<IRoleQuery, RoleQuery>();
            services.AddScoped<ISessionQuery, SessionQuery>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<ISessionTokenExchangeRepository, SessionTokenExchangeRepository>();

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