namespace Shuttle.Access;

public interface IAuthorizationService
{
    Task<IEnumerable<Messages.v1.Permission>> GetPermissionsAsync(string identityName, Guid tenantId, CancellationToken cancellationToken = default);
}