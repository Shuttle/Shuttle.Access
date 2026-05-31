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
        public AccessClientBuilder UseBearerAuthenticationProvider(Action<BearerAuthenticationInterceptorOptions> configureOptions)
        {
            var services = Guard.AgainstNull(accessClientBuilder).Services;

            services.TryAddSingleton<IJwtService, JwtService>();
            services.AddSingleton<IAuthenticationInterceptor, BearerAuthenticationInterceptor>();

            services.Configure(configureOptions);

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

        public AccessClientBuilder UsePasswordAuthenticationProvider(Action<PasswordAuthenticationInterceptorOptions> configureOptions)
        {
            var services = Guard.AgainstNull(accessClientBuilder).Services;

            services.AddHttpClient<IAuthenticationInterceptor, PasswordAuthenticationInterceptor>("PasswordAuthenticationProvider");

            services.Configure(configureOptions);

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