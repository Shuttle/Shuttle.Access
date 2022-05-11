using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IIdentitiesApi
    {
        [Get("/identities")]
        Task<ApiResponse<List<DataAccess.Query.Identity>>> Get();
        
        [Get("/identities/{value}")]
        Task<ApiResponse<DataAccess.Query.Identity>> Get(string value);
        
        [Delete("/identities/{id}")]
        Task<ApiResponse<object>> Delete(Guid id);
        
        [Patch("/identities/{id}/roles/{roleId}")]
        Task<ApiResponse<object>> SetRoleStatus(Guid id, Guid roleId, SetIdentityRoleStatus message);

        [Put("/identities/password/change")]
        Task<ApiResponse<object>> ChangePassword(ChangePassword message);

        [Put("/identities/password/reset")]
        Task<ApiResponse<object>> ResetPassword(ResetPassword message);

        [Get("/identities/{id}/roles")]
        Task<ApiResponse<List<Guid>>> GetRoles(Guid id, DateTime startDateRegistered);

        [Put("/identities/activate")]
        Task<ApiResponse<object>> Activate(ActivateIdentity message);

        [Get("/identities/{name}/password/reset-token")]
        Task<ApiResponse<Guid>> GetPasswordResetToken(string name);

        [Post("/identities")]
        Task<ApiResponse<Guid>> Register(RegisterIdentity message);
    }
}