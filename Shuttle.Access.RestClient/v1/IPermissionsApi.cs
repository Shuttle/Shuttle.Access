using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IPermissionsApi
    {
        [Get("/permissions/anonymous")]
        Task<ApiResponse<AnonymousPermissions>> GetAnonymous();

        [Get("/permissions")]
        Task<ApiResponse<List<string>>> Get();

        [Post("/permissions")]
        Task<ApiResponse<object>> Post(RegisterPermission message);

        [Delete("/permissions/{permission}")]
        Task<ApiResponse<object>> Delete(string permission);
    }
}