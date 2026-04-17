using Refit;

namespace Shuttle.Access.RestClient.v1;

public interface IRolesApi
{
    [Delete("/v1/roles/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/v1/roles/{value}")]
    Task<IApiResponse<WebApi.Contracts.v1.Role>> GetAsync(string value, CancellationToken cancellationToken = default);

    [Post("/v1/roles/{id}/permissions/availability")]
    Task<IApiResponse<List<WebApi.Contracts.v1.IdentifierAvailability<Guid>>>> PermissionAvailabilityAsync(Guid id, WebApi.Contracts.v1.Identifiers<Guid> identifiers, CancellationToken cancellationToken = default);

    [Post("/v1/roles")]
    Task<IApiResponse> RegisterAsync(WebApi.Contracts.v1.RegisterRole message, CancellationToken cancellationToken = default);

    [Post("/v1/roles/search")]
    Task<IApiResponse<List<WebApi.Contracts.v1.Role>>> SearchAsync(WebApi.Contracts.v1.Role.Specification specification, CancellationToken cancellationToken = default);

    [Patch("/v1/roles/{id}/permissions/{permissionId}/status")]
    Task<IApiResponse> SetPermissionStatusAsync(Guid id, Guid permissionId, WebApi.Contracts.v1.SetActiveStatus message, CancellationToken cancellationToken = default);
}