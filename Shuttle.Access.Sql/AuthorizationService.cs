using System.Collections.Generic;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Sql
{
    public class AuthorizationService : IAuthorizationService, IAnonymousPermissions
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISystemRoleQuery _systemRoleQuery;
        private readonly ISystemUserQuery _systemUserQuery;

        public AuthorizationService(IDatabaseContextFactory databaseContextFactory, ISystemRoleQuery systemRoleQuery, ISystemUserQuery systemUserQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(systemRoleQuery, nameof(systemRoleQuery));
            Guard.AgainstNull(systemUserQuery, nameof(systemUserQuery));

            _databaseContextFactory = databaseContextFactory;
            _systemRoleQuery = systemRoleQuery;
            _systemUserQuery = systemUserQuery;
        }

        public IEnumerable<string> AnonymousPermissions()
        {
            var result = new List<string>();
            var count = 0;

            using (_databaseContextFactory.Create())
            {
                count = _systemUserQuery.Count();
                result.AddRange(_systemRoleQuery.Permissions("Anonymous"));
            }

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