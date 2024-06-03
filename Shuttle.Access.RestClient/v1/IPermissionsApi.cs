using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IPermissionsApi
    {
        [Post("/permissions/search")]
        Task<ApiResponse<List<DataAccess.Query.Permission>>> SearchAsync(PermissionSpecification specification);

        [Get("/permissions")]
        Task<ApiResponse<List<DataAccess.Query.Permission>>> GetAsync();

        [Post("/permissions")]
        Task<ApiResponse<object>> PostAsync(RegisterPermission message);

        [Patch("/permissions/{id}")]
        Task<ApiResponse<object>> SetStatusAsync(Guid id, SetPermissionStatus message);
    }
}