using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAccessAuthorization(Action<AccessAuthorizationBuilder>? builder = null)
        {
            Guard.AgainstNull(services);

            var accessAuthorizationBuilder = new AccessAuthorizationBuilder(services);

            builder?.Invoke(accessAuthorizationBuilder);

            services.Configure<AccessAuthorizationOptions>(options =>
            {
                options.AuthorizationHeaderAvailable = accessAuthorizationBuilder.Options.AuthorizationHeaderAvailable;
                options.InsecureModeEnabled = accessAuthorizationBuilder.Options.InsecureModeEnabled;
                options.Issuers = accessAuthorizationBuilder.Options.Issuers;
                options.JwtIssuerOptionsAvailable = accessAuthorizationBuilder.Options.JwtIssuerOptionsAvailable;
                options.JwtIssuerOptionsUnavailable = accessAuthorizationBuilder.Options.JwtIssuerOptionsUnavailable;
                options.PassThrough = accessAuthorizationBuilder.Options.PassThrough;
                options.SessionAvailable = accessAuthorizationBuilder.Options.SessionAvailable;
                options.SessionUnavailable = accessAuthorizationBuilder.Options.SessionUnavailable;
                options.Realm = accessAuthorizationBuilder.Options.Realm;
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<IJwtService, JwtService>();

            services
                .AddSingleton<IValidateOptions<AccessAuthorizationOptions>, AccessAuthorizationOptionsValidator>()
                .AddScoped<AccessAuthorizationMiddleware>()
                .AddScoped<IHttpContextSessionService, HttpContextSessionService>()
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = "Routing";
                })
                .AddScheme<AuthenticationSchemeOptions, RoutingAuthenticationHandler>(RoutingAuthenticationHandler.AuthenticationScheme, _ =>
                {
                })
                .AddScheme<AuthenticationSchemeOptions, JwtBearerAuthenticationHandler>(JwtBearerAuthenticationHandler.AuthenticationScheme, _ =>
                {
                })
                .AddScheme<AuthenticationSchemeOptions, SessionTokenAuthenticationHandler>(SessionTokenAuthenticationHandler.AuthenticationScheme, _ =>
                {
                });

            return services;
        }
    }
}