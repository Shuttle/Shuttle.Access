using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc.Rest
{
    public class RestConfiguration : IRestConfiguration
    {
        public RestConfiguration(string url)
        {
            Guard.AgainstNullOrEmptyString(url, nameof(url));

            Url = url;
        }

        public string Url { get; }
    }
}