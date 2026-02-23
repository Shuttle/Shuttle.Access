using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.WebApi;

public static class TenantEndpoints
{
    private static Messages.v1.Tenant Map(SqlServer.Models.Tenant tenant)
    {
        return new()
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Status = tenant.Status,
            LogoSvg = tenant.LogoSvg,
            LogoUrl = tenant.LogoUrl
        };
    }

    public static WebApplication MapTenantEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/v{version:apiVersion}/tenants/search", Search)
            .WithTags("Tenants")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Tenants.View);

        app.MapGet("/v{version:apiVersion}/tenants/{value}", Get)
            .WithTags("Tenants")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Tenants.View);

        app.MapDelete("/v{version:apiVersion}/tenants/{id:Guid}", Delete)
            .WithTags("Roles")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Tenants.Manage);

        app.MapPost("/v{version:apiVersion}/tenants/", Post)
            .WithTags("Tenants")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Tenants.Register);

        app.MapPatch("/v{version:apiVersion}/tenants/{id:Guid}/status", PatchStatus)
            .WithTags("Tenants")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Tenants.Manage);

        return app;
    }

    private static async Task<IResult> PatchStatus(Guid id, [FromBody] SetTenantStatus message, ISessionContext sessionContext, IMediator mediator)
    {
        try
        {
            message.Id = id;
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        await mediator.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }

    private static async Task<IResult> Post([FromBody] RegisterTenant message, ISessionContext sessionContext, IBus bus, IIdentityQuery identityQuery, CancellationToken cancellationToken)
    {
        try
        {
            message.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        if (await identityQuery.CountAsync(new IdentitySpecification().WithName(message.AdministratorIdentityName), cancellationToken) == 0)
        {
            return Results.BadRequest($"Could not find an identity with name '{message.AdministratorIdentityName}'.");
        }

        await bus.SendAsync(sessionContext.Audit(message), cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> Delete(Guid id, ISessionContext sessionContext, [FromServices] IBus bus, CancellationToken cancellationToken)
    {
        await bus.SendAsync(sessionContext.Audit(new RemoveTenant { Id = id }), cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> Get(string value, ITenantQuery tenantQuery)
    {
        var specification = new TenantSpecification();

        if (Guid.TryParse(value, out var id))
        {
            specification.AddId(id);
        }
        else
        {
            specification.AddName(value);
        }

        var tenant = (await tenantQuery.SearchAsync(specification)).SingleOrDefault();

        return tenant != null
            ? Results.Ok(Map(tenant))
            : Results.BadRequest();
    }

    private static async Task<IResult> Search([FromBody] Messages.v1.Tenant.Specification specification, ITenantQuery tenantQuery)
    {
        var search = new TenantSpecification();

        if (!string.IsNullOrWhiteSpace(specification.NameMatch))
        {
            search.WithNameMatch(specification.NameMatch);
        }

        return Results.Ok((await tenantQuery.SearchAsync(search)).Select(Map).ToList());
    }
}
