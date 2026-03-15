using Refit;

namespace Shuttle.Access.RestClient.v1;

public interface ISessionsApi
{
    [Delete("/v1/sessions")]
    Task<IApiResponse> DeleteAsync(CancellationToken cancellationToken = default);

    [Delete("/v1/sessions/self")]
    Task<IApiResponse> DeleteSelfAsync(CancellationToken cancellationToken = default);

    [Get("/v1/sessions/self")]
    Task<IApiResponse<WebApi.Contracts.v1.Session>> GetSelfAsync(CancellationToken cancellationToken = default);

    [Post("/v1/sessions")]
    Task<IApiResponse<WebApi.Contracts.v1.SessionResponse>> PostAsync(WebApi.Contracts.v1.RegisterSession message, CancellationToken cancellationToken = default);

    [Post("/v1/sessions/delegated")]
    Task<IApiResponse<WebApi.Contracts.v1.SessionResponse>> PostAsync(WebApi.Contracts.v1.RegisterDelegatedSession message, CancellationToken cancellationToken = default);

    [Post("/v1/sessions/search")]
    Task<IApiResponse<IEnumerable<WebApi.Contracts.v1.Session>>> PostSearchAsync(WebApi.Contracts.v1.Session.Specification specification, CancellationToken cancellationToken = default);
}