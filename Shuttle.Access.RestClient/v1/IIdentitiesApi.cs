using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Shuttle.Access.RestClient.v1
{
    public interface IIdentitiesApi
    {
        [Get("/identities")]
        Task<ApiResponse<List<DataAccess.Query.Identity>>> Get();
        [Get("/identities/{value}")]
        Task<ApiResponse<DataAccess.Query.Identity>> Get(string value);
        [Delete("/identities/{id}")]
        Task<ApiResponse<object>> Delete(Guid id);
    }
}