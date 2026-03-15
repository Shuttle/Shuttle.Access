namespace Shuttle.Access;

public interface IAuthorizationService
{
    Task<IEnumerable<Query.Permission>> GetPermissionsAsync(string identityName, Guid tenantId, CancellationToken cancellationToken = default);
}