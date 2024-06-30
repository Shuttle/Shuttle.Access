﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.RestClient.v1
{
    public interface IRolesApi
    {
        [Get("/v1/roles")]
        Task<IApiResponse<List<Messages.v1.Role>>> GetAsync();
        
        [Get("/v1/roles/{value}")]
        Task<IApiResponse<Messages.v1.Role>> GetAsync(string value);
        
        [Delete("/v1/roles/{id}")]
        Task<IApiResponse> DeleteAsync(Guid id);
        
        [Patch("/v1/roles/{id}/permissions")]
        Task<IApiResponse> SetPermissionAsync(Guid id, SetRolePermission message);

        [Post("/v1/roles/{id}/permissions/availability")]
        Task<IApiResponse<List<IdentifierAvailability<Guid>>>> PermissionAvailabilityAsync(Guid id, Identifiers<Guid> identifiers);

        [Post("/v1/roles")]
        Task<IApiResponse<Guid>> RegisterAsync(RegisterRole message);
    }
}