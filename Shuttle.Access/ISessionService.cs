namespace Shuttle.Access;

public interface ISessionService
{
    Task<Messages.v1.Session?> FindAsync(Messages.v1.Session.Specification specification, CancellationToken cancellationToken = default);
}