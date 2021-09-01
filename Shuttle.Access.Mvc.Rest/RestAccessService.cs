using System;
using Shuttle.Access.Application;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc.Rest
{
    public class RestAccessService : CachedAccessService, IAccessService
    {
        private readonly IAccessConfiguration _configuration;
        private readonly IAccessClient _accessClient;

        public RestAccessService(IAccessConfiguration configuration, IAccessClient accessClient)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(accessClient, nameof(accessClient));

            _configuration = configuration;
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

            var session = _accessClient.GetSession(token);

            if (session != null)
            {
                Cache(token, session.Data.Permissions, _configuration.SessionDuration);
            }
        }
    }
}