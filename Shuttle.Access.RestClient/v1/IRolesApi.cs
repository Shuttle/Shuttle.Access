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
        Task<ApiResponse<List<DataAccess.Query.Role>>> Get();
        
        [Get("/roles/{value}")]
        Task<ApiResponse<DataAccess.Query.Role>> Get(string value);
        
        [Delete("/roles/{id}")]
        Task<ApiResponse<object>> Delete(Guid id);
        
        [Patch("/roles/{id}/permissions")]
        Task<ApiResponse<object>> SetPermission(Guid id, SetRolePermission message);

        [Post("/roles/{id}/permissions/availability")]
        Task<ApiResponse<List<IdentifierAvailability<Guid>>>> PermissionAvailability(Guid id, Identifiers<Guid> identifiers);

        [Post("/roles")]
        Task<ApiResponse<Guid>> Register(RegisterRole message);
    }
}