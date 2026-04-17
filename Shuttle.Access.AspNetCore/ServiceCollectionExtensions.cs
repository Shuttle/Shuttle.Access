using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.AspNetCore;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public AccessAuthorizationBuilder AddAccessAuthorization(Action<AccessAuthorizationOptions>? configureOptions= null)
        {
            Guard.AgainstNull(services);

            var builder = new AccessAuthorizationBuilder(services);

            services.AddOptions<AccessAuthorizationOptions>().Configure(options =>
            {
                configureOptions?.Invoke(options);
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IJwtService, JwtService>();

            services
                .AddSingleton<IValidateOptions<AccessAuthorizationOptions>, AccessAuthorizationOptionsValidator>()
                .AddScoped<AccessAuthorizationMiddleware>()
                .AddScoped<ISessionContext, SessionContext>()
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = "Routing";
                })
                .AddScheme<AuthenticationSchemeOptions, RoutingAuthenticationHandler>(RoutingAuthenticationHandler.AuthenticationScheme, null)
                .AddScheme<AuthenticationSchemeOptions, JwtBearerAuthenticationHandler>(JwtBearerAuthenticationHandler.AuthenticationScheme, null)
                .AddScheme<AuthenticationSchemeOptions, SessionTokenAuthenticationHandler>(SessionTokenAuthenticationHandler.AuthenticationScheme, null);

            return builder;
        }
    }
}