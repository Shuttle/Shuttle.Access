namespace Shuttle.Access.Data;

public interface ISessionQuery
{
    ValueTask<bool> ContainsAsync(Models.Session.Specification specification, CancellationToken cancellationToken = default);
    ValueTask<int> CountAsync(Models.Session.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Session>> SearchAsync(Models.Session.Specification specification, CancellationToken cancellationToken = default);
}