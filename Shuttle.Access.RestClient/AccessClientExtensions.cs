using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public static class AccessClientExtensions
    {
        public static bool HasSession(this IAccessClient accessClient)
        {
            Guard.AgainstNull(accessClient, nameof(accessClient));

            return !string.IsNullOrWhiteSpace(accessClient.Token);
        }
    }
}