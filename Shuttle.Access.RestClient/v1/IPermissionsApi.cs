using Refit;

namespace Shuttle.Access.RestClient.v1;

public interface IPermissionsApi
{
    [Post("/v1/permissions")]
    Task<IApiResponse> PostAsync(WebApi.Contracts.v1.RegisterPermission message, CancellationToken cancellationToken = default);

    [Post("/v1/permissions/search")]
    Task<IApiResponse<List<WebApi.Contracts.v1.Permission>>> SearchAsync(WebApi.Contracts.v1.Permission.Specification specification, CancellationToken cancellationToken = default);

    [Patch("/v1/permissions/{id}")]
    Task<IApiResponse> SetStatusAsync(Guid id, WebApi.Contracts.v1.SetPermissionStatus message, CancellationToken cancellationToken = default);
}