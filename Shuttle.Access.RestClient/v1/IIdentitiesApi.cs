using System.Threading.Tasks;
using Refit;
using Shuttle.Access.WebApi.Models.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IIdentitiesApi
    {
        [Get("/identities")]
        Task<ApiResponse<DataAccess.Query.Identity>> Get();
    }
}