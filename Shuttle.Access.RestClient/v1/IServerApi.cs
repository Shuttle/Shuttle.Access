using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IServerApi
    {
        [Get("/v1/server/configuration")]
        Task<ApiResponse<ServerConfiguration>> ConfigurationAsync();
    }
}