using System.Threading.Tasks;
using Refit;
using Shuttle.Access.WebApi.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IServerApi
    {
        [Get("/server/configuration")]
        Task<ApiResponse<ServerConfigurationResponse>> Configuration();
    }
}