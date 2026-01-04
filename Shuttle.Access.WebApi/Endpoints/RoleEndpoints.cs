using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
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

        app.MapPatch("/v{version:apiVersion}/roles/{id}/name", async (Guid id, [FromBody] SetRoleName message, [FromServices] IServiceBus serviceBus) =>
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
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPatch("/v{version:apiVersion}/roles/{id}/permissions", async (Guid id, [FromBody] SetRolePermission message, [FromServices] IServiceBus serviceBus) =>
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

                await serviceBus.SendAsync(message);

                return Results.Accepted();
            })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/{id}/permissions/availability", async (Guid id, [FromBody] Identifiers<Guid> identifiers, [FromServices] IRoleQuery roleQuery) =>
            {
                try
                {
                    identifiers.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                var permissions = (await roleQuery.PermissionsAsync(new SqlServer.Models.Role.Specification().AddRoleId(id).AddPermissionIds(identifiers.Values))).ToList();

                var result = from permissionId in identifiers.Values
                    select new IdentifierAvailability<Guid>
                    {
                        Id = permissionId,
                        Active = permissions.Any(item => item.Id.Equals(permissionId))
                    };

                return Results.Ok(result);
            })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/search", async ([FromServices] IRoleQuery roleQuery, [FromBody] Messages.v1.Role.Specification specification) =>
            {
                var search = new SqlServer.Models.Role.Specification();

                if (!string.IsNullOrWhiteSpace(specification.NameMatch))
                {
                    search.WithNameMatch(specification.NameMatch);
                }

                if (specification.ShouldIncludePermissions)
                {
                    search.IncludePermissions();
                }

                return Results.Ok((await roleQuery.SearchAsync(search)).Select(Map).ToList());
            })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.View);

        app.MapGet("/v{version:apiVersion}/roles/{value}", async (string value, [FromServices] IRoleQuery roleQuery) =>
            {
                var specification = new SqlServer.Models.Role.Specification();

                if (Guid.TryParse(value, out var id))
                {
                    specification.AddRoleId(id);
                }
                else
                {
                    specification.AddName(value);
                }

                var role = (await roleQuery.SearchAsync(specification.IncludePermissions())).FirstOrDefault();

                return role != null
                    ? Results.Ok(Map(role))
                    : Results.BadRequest();
            })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.View);

        app.MapDelete("/v{version:apiVersion}/roles/{id}", async (Guid id, [FromServices] IServiceBus serviceBus) =>
            {
                await serviceBus.SendAsync(new RemoveRole
                {
                    Id = id
                });

                return Results.Accepted();
            })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Remove);

        app.MapPost("/v{version:apiVersion}/roles", async ([FromBody] RegisterRole message, [FromServices] IServiceBus serviceBus) =>
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
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/file", async (HttpContext httpContext, IServiceBus serviceBus) =>
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
                        await serviceBus.SendAsync(registerPermission);
                    }

                    await serviceBus.SendAsync(message);
                }

                return Results.Accepted();
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);
        app.MapPost("/v{version:apiVersion}/roles/bulk-upload", async ([FromServices] IServiceBus serviceBus, List<RegisterRole> messages) =>
            {
                if (!messages.Any())
                {
                    return Results.BadRequest();
                }

                foreach (var message in messages)
                {
                    foreach (var registerPermission in message.Permissions)
                    {
                        await serviceBus.SendAsync(registerPermission);
                    }

                    await serviceBus.SendAsync(message);
                }

                return Results.Accepted();
            })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/bulk-download", async (IRoleQuery roleQuery, List<Guid> ids) =>
            {
                if (!ids.Any())
                {
                    return Results.BadRequest();
                }

                var roles = (await roleQuery.SearchAsync(new SqlServer.Models.Role.Specification().IncludePermissions().AddRoleIds(ids))).ToList();

                var result = roles.Select(item => new
                {
                    item.Name,
                    Permissions = item.RolePermissions.Select(permission => new RegisterPermission
                    {
                        Name = permission.Permission.Name,
                        Description = permission.Permission.Description,
                        Status = permission.Permission.Status
                    }).ToList()
                });

                return Results.File(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), "application/json", "roles.json");
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        return app;
    }
}