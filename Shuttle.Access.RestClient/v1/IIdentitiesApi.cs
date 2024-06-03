using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IIdentitiesApi
    {
        [Get("/identities")]
        Task<ApiResponse<List<DataAccess.Query.Identity>>> GetAsync();
        
        [Get("/identities/{value}")]
        Task<ApiResponse<DataAccess.Query.Identity>> GetAsync(string value);
        
        [Delete("/identities/{id}")]
        Task<ApiResponse<object>> DeleteAsync(Guid id);
        
        [Patch("/identities/{id}/roles/{roleId}")]
        Task<ApiResponse<object>> SetRoleAsync(Guid id, Guid roleId, SetIdentityRole message);

        [Put("/identities/password/change")]
        Task<ApiResponse<object>> ChangePasswordAsync(ChangePassword message);

        [Put("/identities/password/reset")]
        Task<ApiResponse<object>> ResetPasswordAsync(ResetPassword message);

        [Post("/identities/{id}/roles/availability")]
        Task<ApiResponse<List<IdentifierAvailability<Guid>>>> RoleAvailabilityAsync(Guid id, Identifiers<Guid> identifiers);

        [Put("/identities/activate")]
        Task<ApiResponse<object>> ActivateAsync(ActivateIdentity message);

        [Get("/identities/{name}/password/reset-token")]
        Task<ApiResponse<Guid>> GetPasswordResetTokenAsync(string name);

        [Post("/identities")]
        Task<ApiResponse<Guid>> RegisterAsync(RegisterIdentity message);
    }
}