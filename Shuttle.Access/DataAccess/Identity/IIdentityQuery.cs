using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess
{
    public interface IIdentityQuery
    {
        ValueTask<int> CountAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<Query.Identity>> SearchAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default);
        Task<IEnumerable<Guid>> RoleIdsAsync(Query.Identity.Specification specification, CancellationToken cancellationToken = default);
        ValueTask<int> AdministratorCountAsync(CancellationToken cancellationToken = default);
        ValueTask<Guid> IdAsync(string identityName, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> PermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}