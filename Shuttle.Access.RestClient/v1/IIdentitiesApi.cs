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
        [Get("/v1/identities")]
        Task<IApiResponse<List<Messages.v1.Identity>>> GetAsync();
        
        [Get("/v1/identities/{value}")]
        Task<IApiResponse<Messages.v1.Identity>> GetAsync(string value);
        
        [Delete("/v1/identities/{id}")]
        Task<IApiResponse> DeleteAsync(Guid id);
        
        [Patch("/v1/identities/{id}/roles/{roleId}")]
        Task<IApiResponse> SetRoleAsync(Guid id, Guid roleId, SetIdentityRole message);

        [Put("/v1/identities/password/change")]
        Task<IApiResponse> ChangePasswordAsync(ChangePassword message);

        [Put("/v1/identities/password/reset")]
        Task<IApiResponse> ResetPasswordAsync(ResetPassword message);

        [Post("/v1/identities/{id}/roles/availability")]
        Task<IApiResponse<List<IdentifierAvailability<Guid>>>> RoleAvailabilityAsync(Guid id, Identifiers<Guid> identifiers);

        [Put("/v1/identities/activate")]
        Task<IApiResponse> ActivateAsync(ActivateIdentity message);

        [Get("/v1/identities/{name}/password/reset-token")]
        Task<IApiResponse<Guid>> GetPasswordResetTokenAsync(string name);

        [Post("/v1/identities")]
        Task<IApiResponse<Guid>> RegisterAsync(RegisterIdentity message);
    }
}