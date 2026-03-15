using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Hopper;

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

        app.MapGet("/v{version:apiVersion}/permissions/{id:guid}", Get)
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

        app.MapPatch("/v{version:apiVersion}/permissions/{id:guid}/name", PatchName)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPatch("/v{version:apiVersion}/permissions/{id:guid}/description", PatchDescription)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPatch("/v{version:apiVersion}/permissions/{id:guid}", PatchStatus)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Manage);

        return app;
    }

    private static async Task<IResult> PatchStatus(Guid id, Contracts.v1.SetPermissionStatus message, ISessionContext sessionContext, IBus bus)
    {
        message.Id = id;

        await bus.SendAsync(sessionContext.Audit(new Messages.v1.SetPermissionStatus
        {
            Id = message.Id,
            Status = message.Status
        }));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchDescription(Guid id, [FromBody] Contracts.v1.SetPermissionDescription message, ISessionContext sessionContext, IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new Messages.v1.SetPermissionDescription
        {
            Id = message.Id,
            Description = message.Description
        }));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchName(Guid id, Contracts.v1.SetPermissionName message, ISessionContext sessionContext, IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new Messages.v1.SetPermissionName
        {
            Id = message.Id,
            Name = message.Name
        }));

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

    private static async Task<IResult> PostUpload(List<Contracts.v1.RegisterPermission> registerPermissions, ISessionContext sessionContext, IBus bus)
    {
        if (!registerPermissions.Any())
        {
            return Results.BadRequest();
        }

        foreach (var registerPermission in registerPermissions)
        {
            await bus.SendAsync(sessionContext.Audit(new Messages.v1.RegisterPermission
            {
                Id = registerPermission.Id ?? Guid.NewGuid(),
                Name = registerPermission.Name,
                Description = registerPermission.Description,
                Status = registerPermission.Status
            }));
        }

        return Results.Accepted();
    }

    private static async Task<IResult> PostFile(ISessionContext sessionContext, IBus bus, HttpContext httpContext)
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
            await bus.SendAsync(sessionContext.Audit(new Messages.v1.RegisterPermission
            {
                Id = registerPermission.Id ?? Guid.NewGuid(),
                Name = registerPermission.Name,
                Description = registerPermission.Description,
                Status = registerPermission.Status
            }));
        }

        return Results.Accepted();
    }

    private static async Task<IResult> Post(Contracts.v1.RegisterPermission message, ISessionContext sessionContext, IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new Messages.v1.RegisterPermission
        {
            Name = message.Name,
            Description = message.Description,
            Status = message.Status
        }));

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