using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Shuttle.Access.RestClient.v1;

public interface ISessionsApi
{
    [Delete("/v1/sessions")]
    Task<IApiResponse> DeleteAsync();
    [Delete("/v1/sessions/self")]
    Task<IApiResponse> DeleteSelfAsync();
    [Get("/v1/sessions/self")]
    Task<IApiResponse<Messages.v1.SessionResponse>> GetSelfAsync();
    [Post("/v1/sessions/search")]
    Task<IApiResponse<IEnumerable<Messages.v1.SessionResponse>>> PostSearchAsync(Messages.v1.Session.Specification specification);
    [Post("/v1/sessions")]
    Task<IApiResponse<Messages.v1.SessionResponse>> PostAsync(Messages.v1.RegisterSession message);
    [Post("/v1/sessions/delegated")]
    Task<IApiResponse<Messages.v1.SessionResponse>> PostAsync(Messages.v1.RegisterDelegatedSession message);
}