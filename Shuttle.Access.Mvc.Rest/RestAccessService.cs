using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Mvc.Rest
{
    public class RestAccessService : CachedAccessService, IAccessService
    {
        private readonly IAccessConfiguration _configuration;
        private readonly IRestService _restService;

        public RestAccessService(IAccessConfiguration configuration, IRestService restService)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(restService, nameof(restService));

            _configuration = configuration;
            _restService = restService;
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

            var permissions = _restService.GetPermissions(token);

            if (permissions != null)
            {
                Cache(token, permissions, _configuration.SessionDuration);
            }
        }
    }
}