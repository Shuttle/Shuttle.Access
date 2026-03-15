using Refit;

namespace Shuttle.Access.RestClient.v1;

public interface IIdentitiesApi
{
    [Patch("/v1/identities/activate")]
    Task<IApiResponse> ActivateAsync(WebApi.Contracts.v1.ActivateIdentity message, CancellationToken cancellationToken = default);

    [Patch("/v1/identities/password")]
    Task<IApiResponse> ChangePasswordAsync(WebApi.Contracts.v1.ChangePassword message, CancellationToken cancellationToken = default);

    [Delete("/v1/identities/{id}")]
    Task<IApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    [Get("/v1/identities/{value}")]
    Task<IApiResponse<WebApi.Contracts.v1.Identity>> GetAsync(string value, CancellationToken cancellationToken = default);

    [Get("/v1/identities/{id}/password/reset-token")]
    Task<IApiResponse<Guid>> GetPasswordResetTokenAsync(Guid id, CancellationToken cancellationToken = default);

    [Post("/v1/identities")]
    Task<IApiResponse> RegisterAsync(WebApi.Contracts.v1.RegisterIdentity message, CancellationToken cancellationToken = default);

    [Patch("/v1/identities/password/reset")]
    Task<IApiResponse> ResetPasswordAsync(WebApi.Contracts.v1.ResetPassword message, CancellationToken cancellationToken = default);

    [Patch("/v1/identities/{id}/roles/{roleId}")]
    Task<IApiResponse> SetRoleAsync(Guid id, Guid roleId, WebApi.Contracts.v1.SetIdentityRoleStatus message, CancellationToken cancellationToken = default);

    [Post("/v1/identities/{id}/roles/availability")]
    Task<IApiResponse<List<WebApi.Contracts.v1.IdentifierAvailability<Guid>>>> RoleAvailabilityAsync(Guid id, WebApi.Contracts.v1.Identifiers<Guid> identifiers, CancellationToken cancellationToken = default);

    [Patch("/v1/identities/{id}/tenants/{tenantId}")]
    Task<IApiResponse> SetTenantAsync(Guid id, Guid tenantId, WebApi.Contracts.v1.SetIdentityTenantStatus message, CancellationToken cancellationToken = default);

    [Post("/v1/identities/{id}/tenants/availability")]
    Task<IApiResponse<List<WebApi.Contracts.v1.IdentifierAvailability<Guid>>>> TenantAvailabilityAsync(Guid id, WebApi.Contracts.v1.Identifiers<Guid> identifiers, CancellationToken cancellationToken = default);

    [Post("/v1/identities/search")]
    Task<IApiResponse<List<WebApi.Contracts.v1.Identity>>> SearchAsync(WebApi.Contracts.v1.Identity.Specification specification, CancellationToken cancellationToken = default);
}