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
        
        [Post("/identities/setrolestatus")]
        Task<ApiResponse<object>> SetRoleStatus(SetIdentityRoleStatus message);

        [Post("/identities/changepassword")]
        Task<ApiResponse<object>> ChangePassword(ChangePassword message);

        [Post("/identities/resetpassword")]
        Task<ApiResponse<object>> ResetPassword(ResetPassword message);

        [Post("/identities/rolestatus")]
        Task<ApiResponse<List<IdentityRoleStatus>>> GetRoleStatus(GetIdentityRoleStatus message);

        [Post("/identities/activate")]
        Task<ApiResponse<object>> Activate(ActivateIdentity message);

        [Post("/identities/getpasswordresettoken")]
        Task<ApiResponse<Guid>> GetPasswordResetToken(GetPasswordResetToken message);

        [Post("/identities")]
        Task<ApiResponse<Guid>> Register(RegisterIdentity message);
    }
}