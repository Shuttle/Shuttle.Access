﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAccess(this IServiceCollection services, Action<AccessBuilder> builder = null)
        {
            Guard.AgainstNull(services);

            var accessBuilder = new AccessBuilder(services);

            builder?.Invoke(accessBuilder);

            services.AddOptions<AccessOptions>().Configure(options =>
            {
                options.ConnectionStringName = accessBuilder.Options.ConnectionStringName;
                options.SessionDuration = accessBuilder.Options.SessionDuration;
                options.SessionRenewalTolerance = accessBuilder.Options.SessionRenewalTolerance;
                options.SiteUrl = accessBuilder.Options.SiteUrl;
                options.OAuthProviderNames = accessBuilder.Options.OAuthProviderNames;
                options.OAuthRegisterUnknownIdentities = accessBuilder.Options.OAuthRegisterUnknownIdentities;
            });

            return services;
        }
    }
}