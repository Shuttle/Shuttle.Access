using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface ISessionsApi
    {
        [Post("/v1/sessions")]
        Task<IApiResponse<SessionRegistered>> PostAsync(RegisterSession message);

        [Post("/v1/sessions/delegated")]
        Task<IApiResponse<SessionRegistered>> PostAsync(RegisterDelegatedSession message);

        [Delete("/v1/sessions")]
        Task<IApiResponse> DeleteAsync();

        [Get("/v1/sessions/{token}")]
        Task<IApiResponse<DataAccess.Query.Session>> GetAsync(Guid token);

        [Get("/v1/sessions/{token}/permissions")]
        Task<IApiResponse<List<string>>> GetPermissionsAsync(Guid token);
    }
}