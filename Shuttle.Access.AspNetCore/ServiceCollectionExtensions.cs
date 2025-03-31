using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore.Authentication;
using Shuttle.Core.Contract;

namespace Shuttle.Access.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessAuthorization(this IServiceCollection services, Action<AccessAuthorizationBuilder>? builder = null)
    {
        Guard.AgainstNull(services);

        var accessAuthorizationBuilder = new AccessAuthorizationBuilder(services);

        builder?.Invoke(accessAuthorizationBuilder);

        services.Configure<AccessAuthorizationOptions>(options =>
        {
            options.Issuers = accessAuthorizationBuilder.Options.Issuers;
        });

        services
            .AddSingleton<IValidateOptions<AccessAuthorizationOptions>, AccessAuthorizationOptionsValidator>()
            .AddSingleton<AccessAuthorizationMiddleware>()
            .AddSingleton<IJwtService, JwtService>()
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