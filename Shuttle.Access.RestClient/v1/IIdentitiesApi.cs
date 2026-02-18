using Refit;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1;

public interface IIdentitiesApi
{
    [Put("/v1/identities/activate")]
    Task<IApiResponse> ActivateAsync(ActivateIdentity message, CancellationToken cancellationToken = default);

    [Put("/v1/identities/password")]
    Task<IApiResponse> ChangePasswordAsync(ChangePassword message, CancellationToken cancellationToken = default);

    [Delete("/v1/identities/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/v1/identities/{value}")]
    Task<IApiResponse<Messages.v1.Identity>> GetAsync(string value, CancellationToken cancellationToken = default);

    [Get("/v1/identities/{name}/password/reset-token")]
    Task<IApiResponse<Guid>> GetPasswordResetTokenAsync(string name, CancellationToken cancellationToken = default);

    [Post("/v1/identities")]
    Task<IApiResponse<Guid>> RegisterAsync(RegisterIdentity message, CancellationToken cancellationToken = default);

    [Put("/v1/identities/password/reset")]
    Task<IApiResponse> ResetPasswordAsync(ResetPassword message, CancellationToken cancellationToken = default);

    [Patch("/v1/identities/{id}/roles/{roleId}")]
    Task<IApiResponse> SetRoleAsync(Guid id, Guid roleId, SetIdentityRoleStatus message, CancellationToken cancellationToken = default);

    [Post("/v1/identities/{id}/roles/availability")]
    Task<IApiResponse<List<IdentifierAvailability<Guid>>>> RoleAvailabilityAsync(Guid id, Identifiers<Guid> identifiers, CancellationToken cancellationToken = default);

    [Patch("/v1/identities/{id}/tenants/{tenantId}")]
    Task<IApiResponse> SetTenantAsync(Guid id, Guid tenantId, SetIdentityTenantStatus message, CancellationToken cancellationToken = default);

    [Post("/v1/identities/{id}/tenants/availability")]
    Task<IApiResponse<List<IdentifierAvailability<Guid>>>> TenantAvailabilityAsync(Guid id, Identifiers<Guid> identifiers, CancellationToken cancellationToken = default);

    [Post("/v1/identities/search")]
    Task<IApiResponse<List<Messages.v1.Identity>>> SearchAsync(Messages.v1.Identity.Specification specification, CancellationToken cancellationToken = default);
}