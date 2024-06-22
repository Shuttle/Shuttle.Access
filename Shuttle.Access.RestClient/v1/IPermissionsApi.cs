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
        Task<IApiResponse<List<DataAccess.Query.Permission>>> SearchAsync(PermissionSpecification specification);

        [Get("/permissions")]
        Task<IApiResponse<List<DataAccess.Query.Permission>>> GetAsync();

        [Post("/permissions")]
        Task<IApiResponse> PostAsync(RegisterPermission message);

        [Patch("/permissions/{id}")]
        Task<IApiResponse> SetStatusAsync(Guid id, SetPermissionStatus message);
    }
}