using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.WebApi.Contracts.v1;
using Shuttle.Hopper;

namespace Shuttle.Access.WebApi;

public static class RoleEndpoints
{
    private static Contracts.v1.Role Map(Query.Role role)
    {
        return new()
        {
            Id = role.Id,
            Name = role.Name,
            Permissions = role.Permissions.Select(item => new Contracts.v1.Permission
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Status = (int)item.Status
            }).ToList()
        };
    }

    public static WebApplication MapRoleEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPatch("/v{version:apiVersion}/roles/{id:Guid}/name", PatchName)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPatch("/v{version:apiVersion}/roles/{id:Guid}/permissions/{permissionId:Guid}/status", PatchPermissionStatus)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/{id:Guid}/permissions/availability", PostPermissionsAvailability)
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

        app.MapPost("/v{version:apiVersion}/roles/upload", PostUpload)
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
        if (ids.Count == 0)
        {
            return Results.BadRequest();
        }

        var roles = (await roleQuery.SearchAsync(new Query.Role.Specification().IncludePermissions().AddIds(ids))).ToList();

        var result = roles.Select(item => new Contracts.v1.Role
        {
            Id = item.Id,
            Name = item.Name, 
            Permissions = item.Permissions.Select(permission => new Contracts.v1.Permission
            {
                Id = permission.Id,
                Name = permission.Name, 
                Description = permission.Description, 
                Status = (int)permission.Status
            }).ToList()
        });

        return Results.File(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), "application/json", "roles.json");
    }

    private static async Task<IResult> PostUpload(List<Contracts.v1.RegisterRole> messages, ISessionContext sessionContext, [FromServices] IBus bus)
    {
        if (!messages.Any())
        {
            return Results.BadRequest();
        }

        foreach (var message in messages)
        {
            foreach (var registerPermission in message.Permissions)
            {
                await bus.SendAsync(sessionContext.Audit(new Messages.v1.RegisterPermission
                {
                    Id = registerPermission.Id ?? Guid.NewGuid(),
                    Name = registerPermission.Name,
                    Description = registerPermission.Description,
                    Status = registerPermission.Status
                }));
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

        var messages = JsonSerializer.Deserialize<List<Contracts.v1.RegisterRole>>(form.Files[0].OpenReadStream());

        if (messages == null || !messages.Any())
        {
            return Results.BadRequest();
        }

        foreach (var message in messages)
        {
            foreach (var registerPermission in message.Permissions)
            {
                await bus.SendAsync(sessionContext.Audit(new Messages.v1.RegisterPermission
                {
                    Id = registerPermission.Id ?? Guid.NewGuid(),
                    Name = registerPermission.Name,
                    Description = registerPermission.Description,
                    Status = registerPermission.Status
                }));
            }

            await bus.SendAsync(message);
        }

        return Results.Accepted();
    }

    private static async Task<IResult> Post(Contracts.v1.RegisterRole message, ISessionContext sessionContext, IBus bus)
    {
        if (!sessionContext.IsAuthorized)
        {
            return Results.Unauthorized();
        }

        if (Guid.Empty.Equals(message.TenantId))
        {
            message.TenantId = sessionContext.Session!.TenantId!.Value;
        }
        else
        {
            if (!sessionContext.Session.HasPermission(AccessPermissions.Tenants.Manage))
            {
                return Results.Forbid();
            }
        }

        await bus.SendAsync(sessionContext.Audit(new Messages.v1.RegisterRole
        {
            Id = message.Id ?? Guid.NewGuid(),
            Name = message.Name,
            TenantId = message.TenantId,
            Permissions = message.Permissions.Select(item => new Messages.v1.RegisterPermission
            {
                Id = item.Id ?? Guid.NewGuid(),
                Name = item.Name,
                Description = item.Description,
                Status = item.Status
            }).ToList()
        }));

        return Results.Accepted();
    }

    private static async Task<IResult> Delete(Guid id, ISessionContext sessionContext, [FromServices] IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new RemoveRole { Id = id }));

        return Results.Accepted();
    }

    private static async Task<IResult> Get(string value, [FromServices] IRoleQuery roleQuery)
    {
        var specification = new Query.Role.Specification();

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

    private static async Task<IResult> PostSearch(ISessionContext sessionContext, [FromServices] IRoleQuery roleQuery, [FromBody] Contracts.v1.Role.Specification specification)
    {
        if (sessionContext.Session == null ||
            sessionContext.Session.TenantId == null)
        {
            return Results.BadRequest();
        }

        var search = new Query.Role.Specification().WithTenantId(sessionContext.Session.TenantId.Value);

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
            var permissions = (await roleQuery.PermissionsAsync(new Query.Role.Specification().AddId(id).AddPermissionIds(identifiers.Values))).ToList();

            var result = from permissionId in identifiers.Values
                select new IdentifierAvailability<Guid>
                {
                    Id = permissionId,
                    Active = permissions.Any(item => item.Id.Equals(permissionId))
                };

            return Results.Ok(result);
        }

    private static async Task<IResult> PatchPermissionStatus(Guid id, Guid permissionId, [FromBody] Contracts.v1.SetActiveStatus message, ISessionContext sessionContext, [FromServices] IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new SetRolePermissionStatus
        {
            RoleId = id,
            PermissionId = permissionId,
            Active = message.Active
        }));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchName(Guid id, [FromBody] SetName message, ISessionContext sessionContext, [FromServices] IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new SetRoleName
        {
            Id = id,
            Name = message.Name
        }));

        return Results.Accepted();
    }
}