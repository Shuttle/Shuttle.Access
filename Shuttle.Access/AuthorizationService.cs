using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Access.Sql
{
    public class AuthorizationService : IAuthorizationService, IAnonymousPermissions
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly ISystemUserQuery _systemUserQuery;
        private readonly ISystemRoleQuery _systemRoleQuery;

        public AuthorizationService(IDatabaseContextFactory databaseContextFactory, ISystemUserQuery systemUserQuery, ISystemRoleQuery systemRoleQuery)
        {
            Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
            Guard.AgainstNull(systemUserQuery, "systemUserQuery");
            Guard.AgainstNull(systemRoleQuery, "systemRoleQuery");

            _databaseContextFactory = databaseContextFactory;
            _systemUserQuery = systemUserQuery;
            _systemRoleQuery = systemRoleQuery;
        }

        public IEnumerable<string> Permissions(string username, object authenticationTag)
        {
            return new List<string> { "*" };
        }

        public IEnumerable<string> AnonymousPermissions()
        {
            var result = new List<string>();

            using (_databaseContextFactory.Create())
            {
                result.AddRange(_systemRoleQuery.Permissions("Anonymous"));
            }

            return result;
        }
    }
}