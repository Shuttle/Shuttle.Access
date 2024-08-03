﻿using Asp.Versioning;
using Asp.Versioning.Builder;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi;

public static class StatisticEndpoints
{
    public static void MapStatisticEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/statistics/dashboard", async (IDatabaseContextFactory databaseContextFactory, IRoleQuery roleQuery, IIdentityQuery identityQuery, IPermissionQuery permissionQuery) =>
            {
                await using (Guard.AgainstNull(databaseContextFactory).Create())
                {
                    return Results.Ok(new
                    {
                        IdentityCount = await Guard.AgainstNull(identityQuery).CountAsync(new DataAccess.Query.Identity.Specification()),
                        RoleCount = await Guard.AgainstNull(roleQuery).CountAsync(new DataAccess.Query.Role.Specification()),
                        PermissionCount = await Guard.AgainstNull(permissionQuery).CountAsync(new DataAccess.Query.Permission.Specification())
                    });
                }
            })
            .RequiresSession()
            .WithTags("Statistics")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);
    }
}