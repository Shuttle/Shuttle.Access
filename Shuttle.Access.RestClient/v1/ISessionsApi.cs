using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface ISessionsApi
    {
        [Post("/sessions")]
        Task<ApiResponse<SessionRegistered>> PostAsync(RegisterSession message);

        [Post("/sessions/delegated")]
        Task<ApiResponse<SessionRegistered>> PostAsync(RegisterDelegatedSession message);

        [Delete("/sessions")]
        Task<ApiResponse<object>> DeleteAsync();

        [Get("/sessions/{token}")]
        Task<ApiResponse<DataAccess.Query.Session>> GetAsync(Guid token);

        [Get("/sessions/{token}/permissions")]
        Task<ApiResponse<List<string>>> GetPermissionsAsync(Guid token);
    }
}