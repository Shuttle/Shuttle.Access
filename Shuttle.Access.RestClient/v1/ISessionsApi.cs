using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1;

public interface ISessionsApi
{
    [Delete("/v1/sessions")]
    Task<IApiResponse> DeleteAsync(CancellationToken cancellationToken = default);

    [Delete("/v1/sessions/self")]
    Task<IApiResponse> DeleteSelfAsync(CancellationToken cancellationToken = default);

    [Get("/v1/sessions/self")]
    Task<IApiResponse<Messages.v1.Session>> GetSelfAsync(CancellationToken cancellationToken = default);

    [Post("/v1/sessions")]
    Task<IApiResponse<SessionResponse>> PostAsync(RegisterSession message, CancellationToken cancellationToken = default);

    [Post("/v1/sessions/delegated")]
    Task<IApiResponse<SessionResponse>> PostAsync(RegisterDelegatedSession message, CancellationToken cancellationToken = default);

    [Post("/v1/sessions/search")]
    Task<IApiResponse<IEnumerable<Messages.v1.Session>>> PostSearchAsync(Messages.v1.Session.Specification specification, CancellationToken cancellationToken = default);
}