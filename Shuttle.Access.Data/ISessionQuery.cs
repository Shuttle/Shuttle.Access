namespace Shuttle.Access.Data;

public interface ISessionQuery
{
    ValueTask<int> CountAsync(Models.Session.Specification specification, CancellationToken cancellationToken = default); 
    ValueTask<bool> ContainsAsync(Models.Session.Specification specification, CancellationToken cancellationToken = default);
    Task<IEnumerable<Models.Session>> SearchAsync(Models.Session.Specification specification, CancellationToken cancellationToken = default);
}