using Refit;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1;

public interface IIdentitiesApi
{
    [Put("/v1/identities/activate")]
    Task<IApiResponse> ActivateAsync(ActivateIdentity message);

    [Put("/v1/identities/password")]
    Task<IApiResponse> ChangePasswordAsync(ChangePassword message);

    [Delete("/v1/identities/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id);

    [Get("/v1/identities/{value}")]
    Task<IApiResponse<Messages.v1.Identity>> GetAsync(string value);

    [Get("/v1/identities/{name}/password/reset-token")]
    Task<IApiResponse<Guid>> GetPasswordResetTokenAsync(string name);

    [Post("/v1/identities")]
    Task<IApiResponse<Guid>> RegisterAsync(RegisterIdentity message);

    [Put("/v1/identities/password/reset")]
    Task<IApiResponse> ResetPasswordAsync(ResetPassword message);

    [Post("/v1/identities/{id}/roles/availability")]
    Task<IApiResponse<List<IdentifierAvailability<Guid>>>> RoleAvailabilityAsync(Guid id, Identifiers<Guid> identifiers);

    [Post("/v1/identities/search")]
    Task<IApiResponse<List<Messages.v1.Identity>>> SearchAsync(Messages.v1.Identity.Specification specification);

    [Patch("/v1/identities/{id}/roles/{roleId}")]
    Task<IApiResponse> SetRoleAsync(Guid id, Guid roleId, SetIdentityRole message);
}