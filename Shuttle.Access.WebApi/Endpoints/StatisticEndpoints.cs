using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.EntityFrameworkCore;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi;

public static class StatisticEndpoints
{
    public static WebApplication MapStatisticEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapGet("/v{version:apiVersion}/statistics/dashboard", Get)
            .WithTags("Statistics")
            .RequireSession()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static async Task<IResult> Get(AccessDbContext accessDbContext)
    {
        Guard.AgainstNull(accessDbContext);

        return Results.Ok(new
        {
            IdentityCount = await accessDbContext.Identities.CountAsync(),
            RoleCount = await accessDbContext.Roles.CountAsync(),
            PermissionCount = await accessDbContext.Permissions.CountAsync(),
            SessionCount = await accessDbContext.Sessions.CountAsync(),
            TenantCount = await accessDbContext.Tenants.CountAsync()
        });
    }
}