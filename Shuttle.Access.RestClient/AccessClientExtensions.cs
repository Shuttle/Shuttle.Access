using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public static class AccessClientExtensions
    {
        public static bool HasSession(this IAccessClient accessClient)
        {
            Guard.AgainstNull(accessClient, nameof(accessClient));

            return accessClient.Token.HasValue && !accessClient.Token.Equals(Guid.Empty);
        }
    }
}