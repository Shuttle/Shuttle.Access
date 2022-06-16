using System;
using Shuttle.Access.RestClient;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc.Rest
{
    public class RestAccessService : CachedAccessService, IAccessService
    {
        private readonly IAccessSessionConfiguration _sessionConfiguration;
        private readonly IAccessClient _accessClient;

        public RestAccessService(IAccessSessionConfiguration sessionConfiguration, IAccessClient accessClient)
        {
            Guard.AgainstNull(sessionConfiguration, nameof(sessionConfiguration));
            Guard.AgainstNull(accessClient, nameof(accessClient));

            _sessionConfiguration = sessionConfiguration;
            _accessClient = accessClient;
        }

        public new bool Contains(Guid token)
        {
            var result = base.Contains(token);

            if (!result)
            {
                Cache(token);

                return base.Contains(token);
            }

            return true;
        }

        public new bool HasPermission(Guid token, string permission)
        {
            if (!base.Contains(token))
            {
                Cache(token);
            }

            return base.HasPermission(token, permission);
        }

        public new void Remove(Guid token)
        {
            base.Remove(token);
        }

        private void Cache(Guid token)
        {
            if (base.Contains(token))
            {
                return;
            }

            var session = _accessClient.Sessions.Get(token).Result;

            if (session.IsSuccessStatusCode &&
                session.Content != null)
            {
                Cache(token, session.Content.Permissions, _sessionConfiguration.SessionDuration);
            }
        }
    }
}