using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Contract;

namespace Shuttle.Access.RestClient;

public static class AccessClientBuilderExtensions
{
    extension(AccessClientBuilder accessClientBuilder)
    {
        /// <summary>
        ///     Authenticates this application against the Shuttle.Access web API using a JWT bearer token that it
        ///     obtains for itself.
        /// </summary>
        public AccessClientBuilder UseBearerAuthenticationProvider(Action<BearerAuthenticationInterceptorOptions> configureOptions)
        {
            var services = Guard.AgainstNull(accessClientBuilder).Services;

            services.AddSingleton<IAuthenticationInterceptor, BearerAuthenticationInterceptor>();
            services.Configure(configureOptions);

            return accessClientBuilder;
        }

        /// <summary>
        ///     Authenticates this application against the Shuttle.Access web API using its own identity name and
        ///     password.
        /// </summary>
        public AccessClientBuilder UsePasswordAuthenticationProvider(Action<PasswordAuthenticationInterceptorOptions> configureOptions)
        {
            var services = Guard.AgainstNull(accessClientBuilder).Services;

            services.AddHttpClient<IAuthenticationInterceptor, PasswordAuthenticationInterceptor>("PasswordAuthenticationProvider");
            services.Configure(configureOptions);

            services.AddSingleton<IValidateOptions<PasswordAuthenticationInterceptorOptions>, PasswordAuthenticationInterceptorOptionsValidator>();

            return accessClientBuilder;
        }
    }
}
