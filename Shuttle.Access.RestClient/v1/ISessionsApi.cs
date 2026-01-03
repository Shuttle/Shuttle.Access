using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1;

public interface ISessionsApi
{
    [Delete("/v1/sessions")]
    Task<IApiResponse> DeleteAsync();

    [Delete("/v1/sessions/self")]
    Task<IApiResponse> DeleteSelfAsync();

    [Get("/v1/sessions/self")]
    Task<IApiResponse<Messages.v1.Session>> GetSelfAsync();

    [Post("/v1/sessions")]
    Task<IApiResponse<SessionResponse>> PostAsync(RegisterSession message);

    [Post("/v1/sessions/delegated")]
    Task<IApiResponse<SessionResponse>> PostAsync(RegisterDelegatedSession message);

    [Post("/v1/sessions/search")]
    Task<IApiResponse<IEnumerable<Messages.v1.Session>>> PostSearchAsync(Messages.v1.Session.Specification specification);
}