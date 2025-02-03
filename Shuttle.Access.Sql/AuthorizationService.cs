using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Sql;

public class AuthorizationService : IAuthorizationService
{
    private static readonly string AdministratorRoleName = "Administrator";
    private static readonly List<string> AdministratorPermissions = ["*"];
    private readonly IIdentityQuery _identityQuery;

    private readonly IRoleQuery _roleQuery;

    public AuthorizationService(IRoleQuery roleQuery, IIdentityQuery identityQuery)
    {
        _roleQuery = Guard.AgainstNull(roleQuery);
        _identityQuery = Guard.AgainstNull(identityQuery);
    }

    public async Task<IEnumerable<string>> GetPermissionsAsync(string identityName, CancellationToken cancellationToken = default)
    {
        var userId = await _identityQuery.IdAsync(identityName, cancellationToken);
        var user = (await _identityQuery.SearchAsync(new DataAccess.Identity.Specification().WithIdentityId(userId).IncludeRoles(), cancellationToken)).FirstOrDefault();

        if (user == null)
        {
            return Enumerable.Empty<string>();
        }

        return user.Roles.Any(item =>
            item.Name.Equals(AdministratorRoleName, StringComparison.InvariantCultureIgnoreCase))
            ? AdministratorPermissions
            : await _identityQuery.PermissionsAsync(userId, cancellationToken);
    }
}