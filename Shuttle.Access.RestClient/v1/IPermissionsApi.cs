using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.WebApi.Models.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IPermissionsApi
    {
        [Get("/permissions/anonymous")]
        Task<ApiResponse<AnonymousPermissionResponse>> GetAnonymous();

        [Get("/permissions")]
        Task<ApiResponse<List<PermissionModel>>> Get();

        [Post("/permissions")]
        Task<ApiResponse<object>> Post(PermissionModel model);

        [Delete("/permissions")]
        Task<ApiResponse<object>> Delete(PermissionModel model);
    }
}