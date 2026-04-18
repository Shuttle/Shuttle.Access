using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public static class AccessClientBuilderExtensions
{
    extension(AccessClientBuilder accessClientBuilder)
    {
        public AccessClientBuilder UseBearerAuthenticationProvider(Action<BearerAuthenticationInterceptorOptions>? configureOptions = null)
        {
            var services = Guard.AgainstNull(accessClientBuilder).Services;

            services.TryAddSingleton<IJwtService, JwtService>();
            services.AddHttpClient<IAuthenticationInterceptor, BearerAuthenticationInterceptor>("BearerAuthenticationProvider");

            services.Configure<BearerAuthenticationInterceptorOptions>(options =>
            {
                configureOptions?.Invoke(options);
            });

            services.AddOptions<AccessClientOptions>().Configure(options =>
            {
                options.ConfigureHttpRequestAsync = async (httpRequestMessage, serviceProvider) =>
                {
                    var authenticationInterceptor = serviceProvider.GetService<IAuthenticationInterceptor>();

                    if (authenticationInterceptor == null)
                    {
                        throw new InvalidOperationException(Resources.AuthenticationInterceptorException);
                    }

                    await authenticationInterceptor.ConfigureAsync(httpRequestMessage);
                };
            });

            return accessClientBuilder;
        }

        public AccessClientBuilder UsePasswordAuthenticationProvider(Action<PasswordAuthenticationInterceptorOptions>? configureOptions = null)
        {
            var services = Guard.AgainstNull(accessClientBuilder).Services;

            services.AddHttpClient<IAuthenticationInterceptor, PasswordAuthenticationInterceptor>("PasswordAuthenticationProvider");

            services.Configure<PasswordAuthenticationInterceptorOptions>(options =>
            {
                configureOptions?.Invoke(options);
            });

            services.AddSingleton<IValidateOptions<PasswordAuthenticationInterceptorOptions>, PasswordAuthenticationInterceptorOptionsValidator>();

            services.AddOptions<AccessClientOptions>().Configure(options =>
            {
                options.ConfigureHttpRequestAsync = async (httpRequestMessage, serviceProvider) =>
                {
                    var authenticationInterceptor = serviceProvider.GetService<IAuthenticationInterceptor>();

                    if (authenticationInterceptor == null)
                    {
                        throw new InvalidOperationException(Resources.AuthenticationInterceptorException);
                    }

                    await authenticationInterceptor.ConfigureAsync(httpRequestMessage);
                };
            });

            return accessClientBuilder;
        }
    }
}