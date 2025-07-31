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

    public async Task<IEnumerable<DataAccess.Permission>> GetPermissionsAsync(string identityName, CancellationToken cancellationToken = default)
    {
        return await _identityQuery.PermissionsAsync(await _identityQuery.IdAsync(identityName, cancellationToken), cancellationToken);
    }
}