using Refit;

namespace Shuttle.Access.RestClient.v1;

public interface IServerApi
{
    [Get("/v1/server/configuration")]
    Task<IApiResponse<WebApi.Contracts.v1.ServerConfiguration>> ConfigurationAsync(CancellationToken cancellationToken = default);
}