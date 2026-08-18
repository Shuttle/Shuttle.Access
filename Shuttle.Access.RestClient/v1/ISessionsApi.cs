using Refit;

namespace Shuttle.Access.RestClient.v1;

public interface ISessionsApi
{
    [Delete("/v1/sessions")]
    Task<IApiResponse> DeleteAsync(CancellationToken cancellationToken = default);

    [Delete("/v1/sessions/self")]
    Task<IApiResponse> DeleteSelfAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     The session for this application's own identity — the call carries the credential supplied by the
    ///     registered `IAuthenticationInterceptor`.  For the *caller's* session use <see cref="ISelfApi" />, which
    ///     forwards the caller's own credential.
    /// </summary>
    [Get("/v1/sessions/self")]
    Task<IApiResponse<WebApi.Contracts.v1.Session>> GetSelfAsync(CancellationToken cancellationToken = default);

    [Post("/v1/sessions")]
    Task<IApiResponse<WebApi.Contracts.v1.SessionResponse>> PostAsync(WebApi.Contracts.v1.SessionRequest message, CancellationToken cancellationToken = default);

    [Post("/v1/sessions/delegated")]
    Task<IApiResponse<WebApi.Contracts.v1.SessionResponse>> PostAsync(WebApi.Contracts.v1.RegisterDelegatedSession message, CancellationToken cancellationToken = default);

    [Post("/v1/sessions/search")]
    Task<IApiResponse<IEnumerable<WebApi.Contracts.v1.Session>>> PostSearchAsync(WebApi.Contracts.v1.Session.Specification specification, CancellationToken cancellationToken = default);
}