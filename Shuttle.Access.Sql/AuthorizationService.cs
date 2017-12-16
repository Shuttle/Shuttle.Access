using System.Collections.Generic;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access.Sql
{
    public class AuthorizationService : IAuthorizationService, IAnonymousPermissions
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISystemRoleQuery _systemRoleQuery;
        private readonly ISystemUserQuery _systemUserQuery;

        public AuthorizationService(IDatabaseContextFactory databaseContextFactory, ISystemRoleQuery systemRoleQuery, ISystemUserQuery systemUserQuery)
        {
            Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
            Guard.AgainstNull(systemRoleQuery, "systemRoleQuery");
            Guard.AgainstNull(systemUserQuery, "systemUserQuery");

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