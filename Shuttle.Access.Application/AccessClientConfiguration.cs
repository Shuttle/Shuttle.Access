using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Application
{
    public class AccessClientConfiguration : IAccessClientConfiguration
    {
        public Uri Url { get; }
        public string IdentityName { get; }
        public string Password { get; }

        public AccessClientConfiguration(string url, string identityName, string password)
        {
            Guard.AgainstNullOrEmptyString(url, nameof(url));
            Guard.AgainstNullOrEmptyString(identityName, nameof(identityName));
            Guard.AgainstNullOrEmptyString(password, nameof(password));

            Url = new Uri($"{url}{(url.EndsWith("/") ? string.Empty : "/")}");
            IdentityName = identityName;
            Password = password;
        }
    }
}
