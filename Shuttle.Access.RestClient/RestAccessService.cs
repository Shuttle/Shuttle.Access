using System;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.RestClient
{
    public class RestAccessService : CachedAccessService, IAccessService
    {
        private readonly IAccessClient _accessClient;
        private readonly AccessOptions _accessOptions;

        public RestAccessService(IOptions<AccessOptions> accessOptions, IAccessClient accessClient)
        {
            _accessOptions = Guard.AgainstNull(accessOptions).Value;
            _accessClient = Guard.AgainstNull(accessClient);
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

            var session = _accessClient.Sessions.GetAsync(token).Result;

            if (session.IsSuccessStatusCode &&
                session.Content != null)
            {
                Cache(token, session.Content.Permissions, _accessOptions.SessionDuration);
            }
        }
    }
}