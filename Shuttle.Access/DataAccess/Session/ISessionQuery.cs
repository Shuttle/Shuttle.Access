using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess
{
    public interface ISessionQuery
    {
        ValueTask<bool> ContainsAsync(Guid token, CancellationToken cancellationToken = default);
        ValueTask<bool> ContainsAsync(Guid token, string permission, CancellationToken cancellationToken = default);
        Task<Query.Session> GetAsync(Guid token, CancellationToken cancellationToken = default);
        Task<IEnumerable<Query.Session>> SearchAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default);
    }
}