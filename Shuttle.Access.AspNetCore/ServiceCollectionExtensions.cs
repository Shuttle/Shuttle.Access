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

            services.AddOptions<AccessAuthorizationOptions>()
                .Configure(options =>
                {
                    configureOptions?.Invoke(options);
                })
                .ValidateOnStart();

            services.AddSingleton<IValidateOptions<AccessAuthorizationOptions>, AccessAuthorizationOptionsValidator>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ForwardedAuthorizationHttpMessageHandler>();

            services.AddHttpClient(DelegatedSessionResolver.HttpClientName, (serviceProvider, client) =>
                {
                    client.BaseAddress = new(Guard.AgainstEmpty(serviceProvider.GetRequiredService<IOptions<AccessAuthorizationOptions>>().Value.BaseAddress, nameof(AccessAuthorizationOptions.BaseAddress)));
                })
                .AddHttpMessageHandler<ForwardedAuthorizationHttpMessageHandler>();

            services
                .AddScoped<AccessAuthorizationMiddleware>()
                .AddScoped<ISessionContext, SessionContext>()
                .AddScoped<ISessionResolver, DelegatedSessionResolver>()
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = AccessAuthenticationHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, AccessAuthenticationHandler>(AccessAuthenticationHandler.AuthenticationScheme, null);

            return builder;
        }
    }
}
