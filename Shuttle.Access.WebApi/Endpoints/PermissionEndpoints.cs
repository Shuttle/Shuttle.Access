using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi;

public static class PermissionEndpoints
{
    public static WebApplication MapPermissionEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/v{version:apiVersion}/permissions/search", async (IDatabaseContextFactory databaseContextFactory, IPermissionQuery permissionQuery, [FromBody] Messages.v1.Permission.Specification specification) =>
            {
                var search = new DataAccess.Permission.Specification();

                if (!string.IsNullOrWhiteSpace(specification.NameMatch))
                {
                    search.WithNameMatch(specification.NameMatch);
                }

                search.AddIds(specification.Ids);

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var permissions = (await permissionQuery.SearchAsync(search)).ToList();
                    return Results.Ok(permissions);
                }
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequireSession();

        app.MapGet("/v{version:apiVersion}/permissions/{id:guid}", async (Guid id, IDatabaseContextFactory databaseContextFactory, IPermissionQuery permissionQuery) =>
            {
                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var permission = (await permissionQuery.SearchAsync(new DataAccess.Permission.Specification().AddId(id))).SingleOrDefault();
                    return permission != null ? Results.Ok(permission) : Results.BadRequest();
                }
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequireSession();

        app.MapPost("/v{version:apiVersion}/permissions", async (RegisterPermission message, IServiceBus serviceBus) =>
            {
                try
                {
                    message.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                await serviceBus.SendAsync(message);

                return Results.Accepted();
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPatch("/v{version:apiVersion}/permissions/{id:guid}/name", async (Guid id, SetPermissionName message, IServiceBus serviceBus) =>
            {
                try
                {
                    message.Id = id;
                    message.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                await serviceBus.SendAsync(message);

                return Results.Accepted();
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPatch("/v{version:apiVersion}/permissions/{id:guid}", async (Guid id, SetPermissionStatus message, IServiceBus serviceBus) =>
            {
                try
                {
                    message.Id = id;
                    message.ApplyInvariants();

                    Guard.AgainstUndefinedEnum<PermissionStatus>(message.Status);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                await serviceBus.SendAsync(message);

                return Results.Accepted();
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Manage);

        return app;
    }
}