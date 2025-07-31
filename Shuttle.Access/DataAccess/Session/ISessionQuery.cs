using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access.DataAccess;

public interface ISessionQuery
{
    ValueTask<int> CountAsync(Session.Specification specification, CancellationToken cancellationToken = default); 
    ValueTask<bool> ContainsAsync(Session.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Session>> SearchAsync(Session.Specification specification, CancellationToken cancellationToken = default);
}