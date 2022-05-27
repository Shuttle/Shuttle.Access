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
        Task<ApiResponse<List<DataAccess.Query.Permission>>> Search(PermissionSpecification specification);

        [Get("/permissions")]
        Task<ApiResponse<List<DataAccess.Query.Permission>>> Get();

        [Post("/permissions")]
        Task<ApiResponse<object>> Post(RegisterPermission message);

        [Delete("/permissions/{id}")]
        Task<ApiResponse<object>> Delete(Guid id);
    }
}