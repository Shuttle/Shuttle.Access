using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Messages;
using Shuttle.Core.Data;
using Shuttle.Esb;
using Asp.Versioning.Builder;

namespace Shuttle.Access.WebApi.Endpoints;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this WebApplication app, ApiVersionSet versionSet)
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
    .RequiresPermission(Permissions.Register.Role);

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
            .RequiresPermission(Permissions.Register.Role);

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
                var permissions = (await roleQuery.PermissionsAsync(new DataAccess.Query.Role.Specification().AddRoleId(id).AddPermissionIds(identifiers.Values))).ToList();

                return Results.Ok(from permissionId in identifiers.Values
                    select new IdentifierAvailability<Guid>()
                    {
                        Id = permissionId,
                        Active = permissions.Any(item => item.Id.Equals(permissionId))
                    });
            }
        })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Role);

        app.MapGet("/v{version:apiVersion}/roles", async ([FromServices] IDatabaseContextFactory databaseContextFactory, [FromServices] IRoleQuery roleQuery) =>
        {
            await using var context = databaseContextFactory.Create();

            return Results.Ok(await roleQuery.SearchAsync(new DataAccess.Query.Role.Specification()));
        })
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Role);

        app.MapGet("/v{version:apiVersion}/roles/{value}", async (string value, [FromServices] IDatabaseContextFactory databaseContextFactory, [FromServices] IRoleQuery roleQuery) =>
        {
            using (new DatabaseContextScope())
            await using (databaseContextFactory.Create())
            {
                var specification = new DataAccess.Query.Role.Specification();

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
            .RequiresPermission(Permissions.View.Role);

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
            .RequiresPermission(Permissions.Remove.Role);

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
            .RequiresPermission(Permissions.Register.Role);

    }
}