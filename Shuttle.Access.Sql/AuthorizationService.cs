using System.Collections.Generic;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Sql
{
    public class AuthorizationService : IAuthorizationService, IAnonymousPermissions
    {
        private readonly IAccessConfiguration _configuration;
        private readonly ISystemRoleQuery _systemRoleQuery;
        private readonly ISystemUserQuery _systemUserQuery;

        public AuthorizationService(IAccessConfiguration configuration,
            ISystemRoleQuery systemRoleQuery, ISystemUserQuery systemUserQuery)
        {
            Guard.AgainstNull(configuration, nameof(configuration));
            Guard.AgainstNull(systemRoleQuery, nameof(systemRoleQuery));
            Guard.AgainstNull(systemUserQuery, nameof(systemUserQuery));

            _configuration = configuration;
            _systemRoleQuery = systemRoleQuery;
            _systemUserQuery = systemUserQuery;
        }

        public IEnumerable<string> AnonymousPermissions()
        {
            var result = new List<string>();

            var count = _systemUserQuery.Count(new DataAccess.Query.User.Specification());

            result.AddRange(_systemRoleQuery.Permissions("Anonymous"));

            if (count == 0)
            {
                result.Add(SystemPermissions.Manage.Users);
                result.Add(SystemPermissions.Register.UserRequired);
            }

            return result;
        }

        public IEnumerable<string> Permissions(string username, object authenticationTag)
        {
            return new List<string> {"*"};
        }
    }
}