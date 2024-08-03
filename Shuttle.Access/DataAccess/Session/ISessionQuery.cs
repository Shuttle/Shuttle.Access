using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Shuttle.Access.DataAccess.Query;

namespace Shuttle.Access.DataAccess
{
    public interface ISessionQuery
    {
        ValueTask<bool> ContainsAsync(Guid token, CancellationToken cancellationToken = default);
        ValueTask<bool> ContainsAsync(Guid token, string permission, CancellationToken cancellationToken = default);
        Task<Messages.v1.Session> GetAsync(Guid token, CancellationToken cancellationToken = default);
        Task<IEnumerable<Messages.v1.Session>> SearchAsync(Query.Session.Specification specification, CancellationToken cancellationToken = default);
    }
}