using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Sql
{
    public class AuthorizationService : IAuthorizationService, IAnonymousPermissions
    {
        private static readonly string AdministratorRoleName = "Administrator";
        private static readonly List<string> AdministratorPermissions = new List<string> { "*" };
        private readonly IIdentityQuery _identityQuery;

        private readonly IRoleQuery _roleQuery;

        public AuthorizationService(IRoleQuery roleQuery, IIdentityQuery identityQuery)
        {
            Guard.AgainstNull(roleQuery, nameof(roleQuery));
            Guard.AgainstNull(identityQuery, nameof(identityQuery));

            _roleQuery = roleQuery;
            _identityQuery = identityQuery;
        }

        public IEnumerable<string> AnonymousPermissions()
        {
            var result = new List<string>();

            var count = _identityQuery.Count(new DataAccess.Query.Identity.Specification());

            result.AddRange(_roleQuery.Permissions(new DataAccess.Query.Role.Specification().WithRoleName("Anonymous"))
                .Select(item => item.Permission));

            if (count == 0)
            {
                result.Add(Access.Permissions.Register.Identity);
                result.Add(Access.Permissions.Register.IdentityRequired);
            }

            return result;
        }

        public IEnumerable<string> Permissions(string identityName)
        {
            var userId = _identityQuery.Id(identityName);
            var user = _identityQuery.Search(
                new DataAccess.Query.Identity.Specification().WithIdentityId(userId).IncludeRoles()).FirstOrDefault();

            if (user == null)
            {
                return Enumerable.Empty<string>();
            }

            return user.Roles.Any(item =>
                item.Name.Equals(AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase))
                ? AdministratorPermissions
                : _identityQuery.Permissions(userId);
        }
    }
}