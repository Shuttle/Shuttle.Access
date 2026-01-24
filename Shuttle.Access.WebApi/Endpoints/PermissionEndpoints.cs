using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Hopper;

namespace Shuttle.Access.WebApi;

public static class PermissionEndpoints
{
    private static Messages.v1.Permission Map(SqlServer.Models.Permission permission)
    {
        return new()
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            Status = permission.Status
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

        app.MapPost("/v{version:apiVersion}/permissions/bulk-upload", PostBulkUpload)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPost("/v{version:apiVersion}/permissions/bulk-download", PostBulkDownload)
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

    private static async Task<IResult> PatchStatus(Guid id, SetPermissionStatus message, ISessionContext sessionContext, IServiceBus serviceBus)
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

        await serviceBus.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchDescription(Guid id, [FromBody] SetPermissionDescription message, ISessionContext sessionContext, IServiceBus serviceBus)
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

        await serviceBus.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchName(Guid id, SetPermissionName message, ISessionContext sessionContext, IServiceBus serviceBus)
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

        await serviceBus.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }

    private static async Task<IResult> PostBulkDownload(List<Guid> ids, IPermissionQuery permissionQuery)
    {
        if (!ids.Any())
        {
            return Results.BadRequest();
        }

        var permissions = (await permissionQuery.SearchAsync(new SqlServer.Models.Permission.Specification().AddIds(ids))).Select(item => new RegisterPermission { Name = item.Name, Description = item.Description, Status = item.Status })
            .ToList();

        return Results.File(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(permissions)), "application/json", "permissions.json");
    }

    private static async Task<IResult> PostBulkUpload(List<RegisterPermission> registerPermissions, ISessionContext sessionContext, IServiceBus serviceBus)
    {
        if (!registerPermissions.Any())
        {
            return Results.BadRequest();
        }

        foreach (var registerPermission in registerPermissions)
        {
            await serviceBus.SendAsync(sessionContext.Audit(registerPermission));
        }

        return Results.Accepted();
    }

    private static async Task<IResult> PostFile(ISessionContext sessionContext, IServiceBus serviceBus, HttpContext httpContext)
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
            await serviceBus.SendAsync(sessionContext.Audit(registerPermission));
        }

        return Results.Accepted();
    }

    private static async Task<IResult> Post(RegisterPermission message, ISessionContext sessionContext, IServiceBus serviceBus)
    {
        try
        {
            message.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        await serviceBus.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }

    private static async Task<IResult> Get(Guid id, IPermissionQuery permissionQuery)
    {
        var permission = (await permissionQuery.SearchAsync(new SqlServer.Models.Permission.Specification().AddId(id))).SingleOrDefault();
        return permission != null ? Results.Ok(Map(permission)) : Results.BadRequest();
    }

    private static async Task<IResult> PostSearch(IPermissionQuery permissionQuery, [FromBody] Messages.v1.Permission.Specification specification)
    {
        var search = new SqlServer.Models.Permission.Specification();

        if (!string.IsNullOrWhiteSpace(specification.NameMatch))
        {
            search.WithNameMatch(specification.NameMatch);
        }

        search.AddIds(specification.Ids);

        return Results.Ok((await permissionQuery.SearchAsync(search)).Select(Map).ToList());
    }
}