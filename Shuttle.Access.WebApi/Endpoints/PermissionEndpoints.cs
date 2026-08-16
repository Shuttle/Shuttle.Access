using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;

namespace Shuttle.Access.WebApi;

public static class PermissionEndpoints
{
    private static Contracts.v1.Permission Map(Query.Permission permission)
    {
        return new()
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Status = (int)permission.Status,
            StatusName = permission.Status.ToString()
        };
    }

    public static WebApplication MapPermissionEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/v{version:apiVersion}/permissions/search", PostSearch)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequireSession();

        app.MapGet("/v{version:apiVersion}/permissions/{id:Guid}", Get)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequireSession();

        app.MapPost("/v{version:apiVersion}/permissions", Post)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPost("/v{version:apiVersion}/permissions/file", PostFile)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPost("/v{version:apiVersion}/permissions/upload", PostUpload)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPost("/v{version:apiVersion}/permissions/download", PostDownload)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPatch("/v{version:apiVersion}/permissions/{id:Guid}/name", PatchName)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPatch("/v{version:apiVersion}/permissions/{id:Guid}/description", PatchDescription)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPatch("/v{version:apiVersion}/permissions/{id:Guid}/status", PatchStatus)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Manage);

        return app;
    }

    private static async Task<IResult> PatchStatus(Guid id, Contracts.v1.SetStatus message, ISessionContext sessionContext, MessageDispatcher messageDispatcher)
    {
        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.SetPermissionStatus
            {
                Id = id,
                Status = message.Status
            }),
            () => new SetPermissionStatus(id, (PermissionStatus)message.Status, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchDescription(Guid id, [FromBody] Contracts.v1.SetDescription message, ISessionContext sessionContext, MessageDispatcher messageDispatcher)
    {
        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.SetPermissionDescription
            {
                Id = id,
                Description = message.Description
            }),
            () => new SetPermissionDescription(id, message.Description, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchName(Guid id, Contracts.v1.SetName message, ISessionContext sessionContext, MessageDispatcher messageDispatcher)
    {
        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.SetPermissionName
            {
                Id = id,
                Name = message.Name
            }),
            () => new SetPermissionName(id, message.Name, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> PostDownload(List<Guid> ids, IPermissionQuery permissionQuery)
    {
        if (ids.Count == 0)
        {
            return Results.BadRequest();
        }

        var permissions = (await permissionQuery.SearchAsync(new Query.Permission.Specification().AddIds(ids))).Select(item => new Contracts.v1.RegisterPermission
            {
                Id = item.Id,
                Name = item.Name, 
                Description = item.Description, 
                Status = (int)item.Status
            })
            .ToList();

        return Results.File(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(permissions)), "application/json", "permissions.json");
    }

    private static async Task<IResult> PostUpload(List<Contracts.v1.RegisterPermission> registerPermissions, ISessionContext sessionContext, MessageDispatcher messageDispatcher)
    {
        if (!registerPermissions.Any())
        {
            return Results.BadRequest();
        }

        foreach (var registerPermission in registerPermissions)
        {
            var id = registerPermission.Id ?? Guid.NewGuid();

            await messageDispatcher.DispatchAsync(
                () => sessionContext.Audit(new Messages.v1.RegisterPermission
                {
                    Id = id,
                    Name = registerPermission.Name,
                    Description = registerPermission.Description,
                    Status = registerPermission.Status
                }),
                () => new RegisterPermission(id, registerPermission.Name, registerPermission.Description, (PermissionStatus)registerPermission.Status, sessionContext.TenantId, sessionContext.Session.IdentityName));
        }

        return Results.Accepted();
    }

    private static async Task<IResult> PostFile(ISessionContext sessionContext, HttpContext httpContext, MessageDispatcher messageDispatcher)
    {
        var form = httpContext.Request.Form;

        if (form.Files.Count == 0)
        {
            return Results.BadRequest();
        }

        var registerPermissions = JsonSerializer.Deserialize<List<Contracts.v1.RegisterPermission>>(form.Files[0].OpenReadStream());

        if (registerPermissions == null || !registerPermissions.Any())
        {
            return Results.BadRequest();
        }

        foreach (var registerPermission in registerPermissions)
        {
            var id = registerPermission.Id ?? Guid.NewGuid();

            await messageDispatcher.DispatchAsync(
                () => sessionContext.Audit(new Messages.v1.RegisterPermission
                {
                    Id = id,
                    Name = registerPermission.Name,
                    Description = registerPermission.Description,
                    Status = registerPermission.Status
                }),
                () => new RegisterPermission(id, registerPermission.Name, registerPermission.Description, (PermissionStatus)registerPermission.Status, sessionContext.TenantId, sessionContext.Session.IdentityName));
        }

        return Results.Accepted();
    }

    private static async Task<IResult> Post(Contracts.v1.RegisterPermission message, ISessionContext sessionContext, MessageDispatcher messageDispatcher)
    {
        var id = message.Id ?? Guid.NewGuid();

        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.RegisterPermission
            {
                Id = id,
                Name = message.Name,
                Description = message.Description,
                Status = message.Status
            }),
            () => new RegisterPermission(id, message.Name, message.Description, (PermissionStatus)message.Status, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> Get(Guid id, IPermissionQuery permissionQuery)
    {
        var permission = (await permissionQuery.SearchAsync(new Query.Permission.Specification().AddId(id))).SingleOrDefault();
        return permission != null ? Results.Ok(Map(permission)) : Results.BadRequest();
    }

    private static async Task<IResult> PostSearch(IPermissionQuery permissionQuery, [FromBody] Contracts.v1.Permission.Specification specification)
    {
        var search = new Query.Permission.Specification();

        if (!string.IsNullOrWhiteSpace(specification.NameMatch))
        {
            search.WithNameMatch(specification.NameMatch);
        }

        search.AddIds(specification.Ids);

        return Results.Ok((await permissionQuery.SearchAsync(search)).Select(Map).ToList());
    }
}