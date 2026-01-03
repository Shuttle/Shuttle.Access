using Refit;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1;

public interface IRolesApi
{
    [Delete("/v1/roles/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id);

    [Get("/v1/roles/{value}")]
    Task<IApiResponse<Messages.v1.Role>> GetAsync(string value);

    [Post("/v1/roles/{id}/permissions/availability")]
    Task<IApiResponse<List<IdentifierAvailability<Guid>>>> PermissionAvailabilityAsync(Guid id, Identifiers<Guid> identifiers);

    [Post("/v1/roles")]
    Task<IApiResponse<Guid>> RegisterAsync(RegisterRole message);

    [Post("/v1/roles/search")]
    Task<IApiResponse<List<Messages.v1.Role>>> SearchAsync(Messages.v1.Role.Specification specification);

    [Patch("/v1/roles/{id}/permissions")]
    Task<IApiResponse> SetPermissionAsync(Guid id, SetRolePermission message);
}