using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public class AccessClientBuilder
    {
        public AccessClientOptions Options
        {
            get => _accessClientOptions;
            set => _accessClientOptions = value ?? throw new ArgumentNullException(nameof(value));
        }

        private AccessClientOptions _accessClientOptions = new AccessClientOptions();

        public AccessClientBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}