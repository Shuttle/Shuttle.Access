using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IRolesApi
    {
        [Get("/v1/roles")]
        Task<ApiResponse<List<DataAccess.Query.Role>>> GetAsync();
        
        [Get("/v1/roles/{value}")]
        Task<ApiResponse<DataAccess.Query.Role>> GetAsync(string value);
        
        [Delete("/v1/roles/{id}")]
        Task<ApiResponse<object>> DeleteAsync(Guid id);
        
        [Patch("/v1/roles/{id}/permissions")]
        Task<ApiResponse<object>> SetPermissionAsync(Guid id, SetRolePermission message);

        [Post("/v1/roles/{id}/permissions/availability")]
        Task<ApiResponse<List<IdentifierAvailability<Guid>>>> PermissionAvailabilityAsync(Guid id, Identifiers<Guid> identifiers);

        [Post("/v1/roles")]
        Task<ApiResponse<Guid>> RegisterAsync(RegisterRole message);
    }
}