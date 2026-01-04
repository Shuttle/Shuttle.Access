using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1;

public interface IPermissionsApi
{
    [Post("/v1/permissions")]
    Task<IApiResponse> PostAsync(RegisterPermission message, CancellationToken cancellationToken = default);

    [Post("/v1/permissions/search")]
    Task<IApiResponse<List<Messages.v1.Permission>>> SearchAsync(Messages.v1.Permission.Specification specification, CancellationToken cancellationToken = default);

    [Patch("/v1/permissions/{id}")]
    Task<IApiResponse> SetStatusAsync(Guid id, SetPermissionStatus message, CancellationToken cancellationToken = default);
}