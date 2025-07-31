using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess;

public interface IIdentityQuery
{
    ValueTask<int> AdministratorCountAsync(CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(Identity.Specification specification, CancellationToken cancellationToken = default);
    ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default);
    Task<IEnumerable<Permission>> PermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Guid>> RoleIdsAsync(Identity.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Identity>> SearchAsync(Identity.Specification specification, CancellationToken cancellationToken = default);
}