using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public class AccessRestClientBuilder
    {
        public AccessRestClientOptions Options
        {
            get => _accessRestClientOptions;
            set => _accessRestClientOptions = value ?? throw new ArgumentNullException(nameof(value));
        }

        private AccessRestClientOptions _accessRestClientOptions = new AccessRestClientOptions();

        public AccessRestClientBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}