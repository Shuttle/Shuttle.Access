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
        Task<Messages.v1.Session> GetAsync(Guid token, CancellationToken cancellationToken = default);
        Task<IEnumerable<Messages.v1.Session>> SearchAsync(SessionSpecification specification, CancellationToken cancellationToken = default);
    }
}