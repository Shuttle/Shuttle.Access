using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;
using System.Text;
using System.Text.Json;

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
                    return Results.Ok((await permissionQuery.SearchAsync(search)).Select(Map).ToList());
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
                    return permission != null ? Results.Ok(Map(permission)) : Results.BadRequest();
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

        app.MapPost("/v{version:apiVersion}/permissions/file", async (HttpContext httpContext, IServiceBus serviceBus) =>
            {
                var form = httpContext.Request.Form;

                if (form.Files.Count == 0)
                {
                    return Results.BadRequest();
                }

                var registerPermissions = JsonSerializer.Deserialize<List<RegisterPermission>>(form.Files[0].OpenReadStream());

                if (registerPermissions == null || !registerPermissions.Any())
                {
                    return Results.BadRequest();
                }

                foreach (var registerPermission in registerPermissions)
                {
                    await serviceBus.SendAsync(registerPermission);
                }

                return Results.Accepted();
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);
        
        app.MapPost("/v{version:apiVersion}/permissions/bulk-upload", async (IServiceBus serviceBus, List<RegisterPermission> registerPermissions) =>
            {
                if (!registerPermissions.Any())
                {
                    return Results.BadRequest();
                }

                foreach (var registerPermission in registerPermissions)
                {
                    await serviceBus.SendAsync(registerPermission);
                }

                return Results.Accepted();
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPost("/v{version:apiVersion}/permissions/bulk-download", async (IDatabaseContextFactory databaseContextFactory, IPermissionQuery permissionQuery, List<Guid> ids) =>
            {
                if (!ids.Any())
                {
                    return Results.BadRequest();
                }

                List<RegisterPermission> permissions;

                using (new DatabaseContextScope())
                {
                    await using (databaseContextFactory.Create())
                    {
                        permissions = (await permissionQuery.SearchAsync(new DataAccess.Permission.Specification().AddIds(ids)))
                            .Select(item => new RegisterPermission
                            {
                                Name = item.Name,
                                Description = item.Description,
                                Status = item.Status
                            })
                            .ToList();
                    }
                }

                return Results.File(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(permissions)), "application/json", "permissions.json");
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

        app.MapPatch("/v{version:apiVersion}/permissions/{id:guid}/description", async (IServiceBus serviceBus, Guid id, [FromBody] SetPermissionDescription message) =>
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
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

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

    private static Messages.v1.Permission Map(DataAccess.Permission permission)
    {
        return new()
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Status = permission.Status
        };
    }
}