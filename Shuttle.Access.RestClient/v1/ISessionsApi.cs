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
        Task<ApiResponse<SessionRegistered>> PostAsync(RegisterSession message);

        [Post("/v1/sessions/delegated")]
        Task<ApiResponse<SessionRegistered>> PostAsync(RegisterDelegatedSession message);

        [Delete("/v1/sessions")]
        Task<ApiResponse<object>> DeleteAsync();

        [Get("/v1/sessions/{token}")]
        Task<ApiResponse<DataAccess.Query.Session>> GetAsync(Guid token);

        [Get("/v1/sessions/{token}/permissions")]
        Task<ApiResponse<List<string>>> GetPermissionsAsync(Guid token);
    }
}