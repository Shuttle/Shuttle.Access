using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.WebApi.Models.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface ISessionsApi
    {
        [Post("/sessions")]
        Task<ApiResponse<RegisterSessionResponse>> Post(RegisterSessionRequest request);

        [Post("/sessions/delegated")]
        Task<ApiResponse<RegisterSessionResponse>> Post(RegisterDelegatedSessionRequest request);

        [Delete("/sessions")]
        Task<ApiResponse<object>> Delete();

        [Get("/sessions/{token}")]
        Task<ApiResponse<DataAccess.Query.Session>> Get(Guid token);

        [Get("/sessions/{token}/permissions")]
        Task<ApiResponse<List<DataAccess.Query.Session>>> GetPermissions(Guid token);
    }
}