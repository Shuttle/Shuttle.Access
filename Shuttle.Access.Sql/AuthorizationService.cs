using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Sql;

public class AuthorizationService : IAuthorizationService
{
    private readonly IIdentityQuery _identityQuery;

    public AuthorizationService(IIdentityQuery identityQuery)
    {
        _identityQuery = Guard.AgainstNull(identityQuery);
    }

    public async Task<IEnumerable<string>> GetPermissionsAsync(string identityName, CancellationToken cancellationToken = default)
    {
        var userId = await _identityQuery.IdAsync(identityName, cancellationToken);
        var user = (await _identityQuery.SearchAsync(new DataAccess.Identity.Specification().WithIdentityId(userId).IncludeRoles(), cancellationToken)).FirstOrDefault();

        if (user == null)
        {
            return [];
        }

        return await _identityQuery.PermissionsAsync(userId, cancellationToken);
    }
}