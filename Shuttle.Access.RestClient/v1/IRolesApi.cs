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
        [Get("/roles")]
        Task<ApiResponse<List<DataAccess.Query.Role>>> GetAsync();
        
        [Get("/roles/{value}")]
        Task<ApiResponse<DataAccess.Query.Role>> GetAsync(string value);
        
        [Delete("/roles/{id}")]
        Task<ApiResponse<object>> DeleteAsync(Guid id);
        
        [Patch("/roles/{id}/permissions")]
        Task<ApiResponse<object>> SetPermissionAsync(Guid id, SetRolePermission message);

        [Post("/roles/{id}/permissions/availability")]
        Task<ApiResponse<List<IdentifierAvailability<Guid>>>> PermissionAvailabilityAsync(Guid id, Identifiers<Guid> identifiers);

        [Post("/roles")]
        Task<ApiResponse<Guid>> RegisterAsync(RegisterRole message);
    }
}