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
    }
}