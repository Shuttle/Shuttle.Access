using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Shuttle.Access.RestClient.v1
{
    public interface IIdentitiesApi
    {
        [Get("/identities")]
        Task<ApiResponse<List<DataAccess.Query.Identity>>> Get();
    }
}