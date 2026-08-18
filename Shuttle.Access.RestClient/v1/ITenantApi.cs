using Refit;

namespace Shuttle.Access.RestClient.v1;

public interface ITenantApi
{
    [Delete("/v1/tenants/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/v1/tenants/{id}")]
    Task<IApiResponse<WebApi.Contracts.v1.Tenant>> GetAsync(Guid id, CancellationToken cancellationToken = default);

    [Post("/v1/tenants/")]
    Task<IApiResponse> RegisterAsync(WebApi.Contracts.v1.RegisterTenant message, CancellationToken cancellationToken = default);

    [Post("/v1/tenants/search")]
    Task<IApiResponse<List<WebApi.Contracts.v1.Tenant>>> SearchAsync(WebApi.Contracts.v1.Tenant.Specification specification, CancellationToken cancellationToken = default);

    [Patch("/v1/tenants/{id}/status")]
    Task<IApiResponse> SetStatusAsync(Guid id, WebApi.Contracts.v1.SetStatus message, CancellationToken cancellationToken = default);
}
