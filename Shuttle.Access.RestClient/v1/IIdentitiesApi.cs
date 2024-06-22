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
        Task<IApiResponse<List<DataAccess.Query.Identity>>> GetAsync();
        
        [Get("/identities/{value}")]
        Task<IApiResponse<DataAccess.Query.Identity>> GetAsync(string value);
        
        [Delete("/identities/{id}")]
        Task<IApiResponse> DeleteAsync(Guid id);
        
        [Patch("/identities/{id}/roles/{roleId}")]
        Task<IApiResponse> SetRoleAsync(Guid id, Guid roleId, SetIdentityRole message);

        [Put("/identities/password/change")]
        Task<IApiResponse> ChangePasswordAsync(ChangePassword message);

        [Put("/identities/password/reset")]
        Task<IApiResponse> ResetPasswordAsync(ResetPassword message);

        [Post("/identities/{id}/roles/availability")]
        Task<IApiResponse<List<IdentifierAvailability<Guid>>>> RoleAvailabilityAsync(Guid id, Identifiers<Guid> identifiers);

        [Put("/identities/activate")]
        Task<IApiResponse> ActivateAsync(ActivateIdentity message);

        [Get("/identities/{name}/password/reset-token")]
        Task<IApiResponse<Guid>> GetPasswordResetTokenAsync(string name);

        [Post("/identities")]
        Task<IApiResponse<Guid>> RegisterAsync(RegisterIdentity message);
    }
}