namespace Shuttle.Access;

public interface ISessionService
{
    Task AddAsync(Guid? token, Messages.v1.Session session, CancellationToken cancellationToken = default);
    Task<Messages.v1.Session?> FindAsync(Messages.v1.Session.Specification specification, CancellationToken cancellationToken = default);
}