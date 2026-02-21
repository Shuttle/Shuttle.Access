using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Hopper;

namespace Shuttle.Access.WebApi;

public static class RoleEndpoints
{
    private static Messages.v1.Role Map(SqlServer.Models.Role role)
    {
        return new()
        {
            Id = role.Id,
            Name = role.Name,
            Permissions = role.RolePermissions.Select(item => new Messages.v1.Role.Permission
            {
                Id = item.PermissionId,
                Name = item.Permission.Name,
                RoleId = role.Id,
                Description = item.Permission.Description,
                Status = item.Permission.Status
            }).ToList()
        };
    }

    public static WebApplication MapRoleEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPatch("/v{version:apiVersion}/roles/{id}/name", PatchName)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPatch("/v{version:apiVersion}/roles/{id}/permissions", PatchPermissions)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/{id}/permissions/availability", PostPermissionsAvailability)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/search", PostSearch)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.View);

        app.MapGet("/v{version:apiVersion}/roles/{value}", Get)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.View);

        app.MapDelete("/v{version:apiVersion}/roles/{id:Guid}", Delete)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Remove);

        app.MapPost("/v{version:apiVersion}/roles", Post)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/file", PostFile)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        app.MapPost("/v{version:apiVersion}/roles/upload", PostBulkUpload)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/download", PostDownload)
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        return app;
    }

    private static async Task<IResult> PostDownload(List<Guid> ids, IRoleQuery roleQuery)
    {
        if (!ids.Any())
        {
            return Results.BadRequest();
        }

        var roles = (await roleQuery.SearchAsync(new RoleSpecification().IncludePermissions().AddIds(ids))).ToList();

        var result = roles.Select(item => new { item.Name, Permissions = item.RolePermissions.Select(permission => new RegisterPermission { Name = permission.Permission.Name, Description = permission.Permission.Description, Status = permission.Permission.Status }).ToList() });

        return Results.File(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), "application/json", "roles.json");
    }

    private static async Task<IResult> PostBulkUpload(List<RegisterRole> messages, ISessionContext sessionContext, [FromServices] IBus bus)
    {
        if (!messages.Any())
        {
            return Results.BadRequest();
        }

        foreach (var message in messages)
        {
            foreach (var registerPermission in message.Permissions)
            {
                await bus.SendAsync(sessionContext.Audit(registerPermission));
            }

            await bus.SendAsync(message);
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

        var messages = JsonSerializer.Deserialize<List<RegisterRole>>(form.Files[0].OpenReadStream());

        if (messages == null || !messages.Any())
        {
            return Results.BadRequest();
        }

        foreach (var message in messages)
        {
            foreach (var registerPermission in message.Permissions)
            {
                await bus.SendAsync(sessionContext.Audit(registerPermission));
            }

            await bus.SendAsync(message);
        }

        return Results.Accepted();
    }

    private static async Task<IResult> Post(RegisterRole message, ISessionContext sessionContext, IBus bus)
    {
        try
        {
            message.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        sessionContext.Audit(message);

        await bus.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }

    private static async Task<IResult> Delete(Guid id, ISessionContext sessionContext, [FromServices] IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new RemoveRole { Id = id }));

        return Results.Accepted();
    }

    private static async Task<IResult> Get(string value, [FromServices] IRoleQuery roleQuery)
    {
        var specification = new RoleSpecification();

        if (Guid.TryParse(value, out var id))
        {
            specification.AddId(id);
        }
        else
        {
            specification.AddName(value);
        }

        var role = (await roleQuery.SearchAsync(specification.IncludePermissions())).FirstOrDefault();

        return role != null
            ? Results.Ok(Map(role))
            : Results.BadRequest();
    }

    private static async Task<IResult> PostSearch(ISessionContext sessionContext, [FromServices] IRoleQuery roleQuery, [FromBody] Messages.v1.Role.Specification specification)
    {
        if (sessionContext.Session == null ||
            sessionContext.Session.TenantId == null)
        {
            return Results.BadRequest();
        }

        var search = new RoleSpecification().WithTenantId(sessionContext.Session.TenantId.Value);

        if (!string.IsNullOrWhiteSpace(specification.NameMatch))
        {
            search.WithNameMatch(specification.NameMatch);
        }

        if (specification.ShouldIncludePermissions)
        {
            search.IncludePermissions();
        }

        return Results.Ok((await roleQuery.SearchAsync(search)).Select(Map).ToList());
    }

    private static async Task<IResult> PostPermissionsAvailability(Guid id, [FromBody] Identifiers<Guid> identifiers, [FromServices] IRoleQuery roleQuery)
        {
            try
            {
                identifiers.ApplyInvariants();
            }
            catch (Exception ex)
            {
                return Results.BadRequest(ex.Message);
            }

            var permissions = (await roleQuery.PermissionsAsync(new RoleSpecification().AddId(id).AddPermissionIds(identifiers.Values))).ToList();

            var result = from permissionId in identifiers.Values
                select new IdentifierAvailability<Guid>
                {
                    Id = permissionId,
                    Active = permissions.Any(item => item.Id.Equals(permissionId))
                };

            return Results.Ok(result);
        }

    private static async Task<IResult> PatchPermissions(Guid id, [FromBody] SetRolePermissionStatus message, ISessionContext sessionContext, [FromServices] IBus bus)
    {
        try
        {
            message.RoleId = id;
            message.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        await bus.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchName(Guid id, [FromBody] SetRoleName message, ISessionContext sessionContext, [FromServices] IBus bus)
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

        await bus.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }
}