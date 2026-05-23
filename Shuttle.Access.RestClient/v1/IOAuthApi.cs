using Refit;

namespace Shuttle.Access.RestClient.v1;

public interface IOAuthApi
{
    [Get("/v1/oauth/identity/{state}/{**code}")]
    Task<IApiResponse<WebApi.Contracts.v1.OAuthIdentity>> GetIdentity(string state, string code, CancellationToken cancellationToken = default);
}