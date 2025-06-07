using System.Threading;
using System.Threading.Tasks;

namespace Shuttle.Access;

public interface IContextSessionService
{
    Task<Messages.v1.Session?> FindAsync(CancellationToken cancellationToken = default);
}

public class NullContextSessionService : IContextSessionService
{
    public async Task<Messages.v1.Session?> FindAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult<Messages.v1.Session?>(null);
    }
}