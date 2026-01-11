using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAccess(Action<AccessBuilder>? builder = null)
        {
            Guard.AgainstNull(services);

            var accessBuilder = new AccessBuilder(services);

            builder?.Invoke(accessBuilder);

            services.AddSingleton<IValidateOptions<AccessOptions>, AccessOptionsValidator>();

            services.AddOptions<AccessOptions>().Configure(options =>
            {
                options.SessionDuration = accessBuilder.Options.SessionDuration;
                options.SessionRenewalTolerance = accessBuilder.Options.SessionRenewalTolerance;
                options.OAuthRegisterUnknownIdentities = accessBuilder.Options.OAuthRegisterUnknownIdentities;
                options.ExtensionFolder = accessBuilder.Options.ExtensionFolder;
                options.Realm = accessBuilder.Options.Realm;
                options.KnownApplications = accessBuilder.Options.KnownApplications;
                options.Configuration = accessBuilder.Options.Configuration;
                options.AllowPasswordAuthentication = accessBuilder.Options.AllowPasswordAuthentication;
            });

            return services;
        }
    }
}