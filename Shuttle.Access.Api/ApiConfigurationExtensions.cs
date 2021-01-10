using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Api
{
    public static class ApiConfigurationExtensions
    {
        public static Uri GetApiUrl(this IClientConfiguration configuration, string path)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNullOrEmptyString(path, nameof(path));

            return new Uri(configuration.Url, path);
        }
    }
}