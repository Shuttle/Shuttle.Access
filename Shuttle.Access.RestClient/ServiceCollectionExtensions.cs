using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAccessClient(Action<AccessClientBuilder>? builder = null)
        {
            Guard.AgainstNull(services);

            var accessClientBuilder = new AccessClientBuilder(services);

            builder?.Invoke(accessClientBuilder);

            services.Configure<AccessClientOptions>(options =>
            {
                options.BaseAddress = accessClientBuilder.Options.BaseAddress;
                options.RenewToleranceTimeSpan = accessClientBuilder.Options.RenewToleranceTimeSpan;
                options.ConfigureHttpRequestAsync = accessClientBuilder.Options.ConfigureHttpRequestAsync;
            });

            services.AddSingleton<IValidateOptions<AccessClientOptions>, AccessClientOptionsValidator>();

            services.AddTransient<AccessHttpMessageHandler>();
            services.AddHttpClient<IAccessClient, AccessClient>("AccessClient", (serviceProvider, client) =>
                {
                    client.BaseAddress = new(serviceProvider.GetRequiredService<IOptions<AccessClientOptions>>().Value.BaseAddress);
                })
                .AddHttpMessageHandler<AccessHttpMessageHandler>();

            services.TryAddSingleton<ISessionCache, SessionCache>();
            
            services
                .AddSingleton<RestSessionService>()
                .AddSingleton<ISessionService>(sp => sp.GetRequiredService<RestSessionService>())
                .AddSingleton<IContextSessionService>(sp => sp.GetRequiredService<RestSessionService>());

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            return services;
        }
    }

    extension(AccessClientBuilder accessClientBuilder)
    {
        public AccessClientBuilder UseBearerAuthenticationProvider(Action<BearerAuthenticationInterceptorBuilder>? builder = null)
        {
            Guard.AgainstNull(accessClientBuilder);

            accessClientBuilder.Services.TryAddSingleton<IJwtService, JwtService>();
            accessClientBuilder.Services.AddHttpClient<IAuthenticationInterceptor, BearerAuthenticationInterceptor>("BearerAuthenticationProvider");

            var bearerAuthenticationProviderBuilder = new BearerAuthenticationInterceptorBuilder(accessClientBuilder.Services);

            builder?.Invoke(bearerAuthenticationProviderBuilder);

            bearerAuthenticationProviderBuilder.Services
                .Configure<BearerAuthenticationInterceptorOptions>(options =>
                {
                    options.GetBearerAuthenticationContextAsync = bearerAuthenticationProviderBuilder.Options.GetBearerAuthenticationContextAsync;
                });

            accessClientBuilder.Options.ConfigureHttpRequestAsync = async (httpRequestMessage, serviceProvider) =>
            {
                var authenticationInterceptor = serviceProvider.GetService<IAuthenticationInterceptor>();

                if (authenticationInterceptor == null)
                {
                    throw new InvalidOperationException(Resources.AuthenticationInterceptorException);
                }

                await authenticationInterceptor.ConfigureAsync(httpRequestMessage);
            };

            return accessClientBuilder;
        }

        public AccessClientBuilder UsePasswordAuthenticationProvider(Action<PasswordAuthenticationInterceptorBuilder>? builder = null)
        {
            Guard.AgainstNull(accessClientBuilder).Services
                .AddHttpClient<IAuthenticationInterceptor, PasswordAuthenticationInterceptor>("PasswordAuthenticationProvider");

            var passwordAuthenticationProviderBuilder = new PasswordAuthenticationInterceptorBuilder(accessClientBuilder.Services);

            builder?.Invoke(passwordAuthenticationProviderBuilder);

            passwordAuthenticationProviderBuilder.Services
                .Configure<PasswordAuthenticationInterceptorOptions>(options =>
                {
                    options.IdentityName = passwordAuthenticationProviderBuilder.Options.IdentityName;
                    options.Password = passwordAuthenticationProviderBuilder.Options.Password;
                });

            passwordAuthenticationProviderBuilder.Services.AddSingleton<IValidateOptions<PasswordAuthenticationInterceptorOptions>, PasswordAuthenticationInterceptorOptionsValidator>();

            accessClientBuilder.Options.ConfigureHttpRequestAsync = async (httpRequestMessage, serviceProvider) =>
            {
                var authenticationInterceptor = serviceProvider.GetService<IAuthenticationInterceptor>();

                if (authenticationInterceptor == null)
                {
                    throw new InvalidOperationException(Resources.AuthenticationInterceptorException);
                }

                await authenticationInterceptor.ConfigureAsync(httpRequestMessage);
            };

            return accessClientBuilder;
        }
    }
}