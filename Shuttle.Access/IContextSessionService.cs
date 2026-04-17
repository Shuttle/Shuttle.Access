namespace Shuttle.Access;

public interface IContextSessionService
{
    Task<Query.Session?> FindAsync(CancellationToken cancellationToken = default);
}

public class NullContextSessionService : IContextSessionService
{
    public Task<Query.Session?> FindAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Query.Session?>(null);
    }
}