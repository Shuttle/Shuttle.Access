using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi;

public static class RoleEndpoints
{
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

        app.MapPost("/v{version:apiVersion}/roles/{id}/permissions/availability", async (Guid id, [FromBody] Identifiers<Guid> identifiers, [FromServices] IDatabaseContextFactory databaseContextFactory, [FromServices] IRoleQuery roleQuery) =>
            {
                try
                {
                    identifiers.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var permissions = (await roleQuery.PermissionsAsync(new DataAccess.Role.Specification().AddRoleId(id).AddPermissionIds(identifiers.Values))).ToList();

                    var result = from permissionId in identifiers.Values
                        select new IdentifierAvailability<Guid>
                        {
                            Id = permissionId,
                            Active = permissions.Any(item => item.Id.Equals(permissionId))
                        };

                    return Results.Ok(result);
                }
            })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/search", async ([FromServices] IDatabaseContextFactory databaseContextFactory, [FromServices] IRoleQuery roleQuery, [FromBody] Messages.v1.Role.Specification specification) =>
            {
                var search = new DataAccess.Role.Specification();

                if (!string.IsNullOrWhiteSpace(specification.NameMatch))
                {
                    search.WithNameMatch(specification.NameMatch);
                }

                if (specification.ShouldIncludePermissions)
                {
                    search.IncludePermissions();
                }

                await using var context = databaseContextFactory.Create();

                return Results.Ok(await roleQuery.SearchAsync(search));
            })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.View);

        app.MapGet("/v{version:apiVersion}/roles/{value}", async (string value, [FromServices] IDatabaseContextFactory databaseContextFactory, [FromServices] IRoleQuery roleQuery) =>
            {
                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var specification = new DataAccess.Role.Specification();

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
                        ? Results.Ok(role)
                        : Results.BadRequest();
                }
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

        app.MapPost("/v{version:apiVersion}/roles/bulk", async ([FromServices] IServiceBus serviceBus, List<RegisterRole> messages) =>
            {
                if (!messages.Any())
                {
                    return Results.BadRequest();
                }

                foreach (var message in messages)
                {
                    foreach (var permission in message.Permissions.Where(item => !string.IsNullOrWhiteSpace(item)))
                    {
                        await serviceBus.SendAsync(new RegisterPermission
                        {
                            Name = permission,
                            Status = 1
                        });
                    }

                    await serviceBus.SendAsync(message);
                }

                return Results.Accepted();
            })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/roles/bulk-download", async (IDatabaseContextFactory databaseContextFactory, IRoleQuery roleQuery, List<Guid> ids) =>
            {
                if (!ids.Any())
                {
                    return Results.BadRequest();
                }

                List<Messages.v1.Role> roles;

                using (new DatabaseContextScope())
                {
                    await using (databaseContextFactory.Create())
                    {
                        roles = (await roleQuery.SearchAsync(new DataAccess.Role.Specification().IncludePermissions().AddRoleIds(ids))).ToList();
                    }
                }

                var result = roles.Select(item => new
                {
                    item.Name,
                    Permissions = item.Permissions.Select(permission => permission.Name).ToList()
                });

                return Results.File(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(result)), "application/json", "permissions.json");
            })
            .WithTags("Permissions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Permissions.Register);

        return app;
    }
}