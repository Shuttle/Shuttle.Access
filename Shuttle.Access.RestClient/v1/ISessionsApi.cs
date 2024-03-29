﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface ISessionsApi
    {
        [Post("/sessions")]
        Task<ApiResponse<SessionRegistered>> Post(RegisterSession message);

        [Post("/sessions/delegated")]
        Task<ApiResponse<SessionRegistered>> Post(RegisterDelegatedSession message);

        [Delete("/sessions")]
        Task<ApiResponse<object>> Delete();

        [Get("/sessions/{token}")]
        Task<ApiResponse<DataAccess.Query.Session>> Get(Guid token);

        [Get("/sessions/{token}/permissions")]
        Task<ApiResponse<List<string>>> GetPermissions(Guid token);
    }
}