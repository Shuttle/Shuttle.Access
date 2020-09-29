using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Sql
{
    public class AuthorizationService : IAuthorizationService, IAnonymousPermissions
    {
        private static readonly string AdministratorRoleName = "Administrator";
        private static readonly List<string> AdministratorPermissions = new List<string> {"*"};

        private readonly ISystemRoleQuery _systemRoleQuery;
        private readonly ISystemUserQuery _systemUserQuery;

        public AuthorizationService(ISystemRoleQuery systemRoleQuery, ISystemUserQuery systemUserQuery)
        {
            Guard.AgainstNull(systemRoleQuery, nameof(systemRoleQuery));
            Guard.AgainstNull(systemUserQuery, nameof(systemUserQuery));

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
            var userId = _systemUserQuery.Id(username);
            var user = _systemUserQuery.Search(
                new DataAccess.Query.User.Specification().WithUserId(userId).IncludeRoles()).FirstOrDefault();

            if (user == null)
            {
                return Enumerable.Empty<string>();
            }

            return user.Roles.Any(item =>
                item.Name.Equals(AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase))
                ? AdministratorPermissions
                : _systemUserQuery.Permissions(userId);
        }
    }
}