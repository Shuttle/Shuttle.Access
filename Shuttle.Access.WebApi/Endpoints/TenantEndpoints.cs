using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.WebApi;

public static class TenantEndpoints
{
    private static async Task<IResult> Delete(Guid id, ISessionContext sessionContext, [FromServices] IBus bus, CancellationToken cancellationToken)
    {
        await bus.SendAsync(sessionContext.Audit(new Messages.v1.RemoveTenant { Id = id }), cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> Get(string value, ITenantQuery tenantQuery)
    {
        var specification = new Query.Tenant.Specification();

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

    private static Contracts.v1.Tenant Map(Query.Tenant tenant)
    {
        return new()
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Status = (int)tenant.Status,
            StatusName = tenant.Status.ToString(),
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

    private static async Task<IResult> PatchStatus(Guid id, [FromBody] Contracts.v1.SetStatus message, ISessionContext sessionContext, IBus bus, CancellationToken cancellationToken)
    {
        await bus.SendAsync(sessionContext.Audit(new Messages.v1.SetTenantStatus
        {
            Id = id,
            Status = message.Status
        }), cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> Post([FromBody] Contracts.v1.RegisterTenant message, ISessionContext sessionContext, IBus bus, IIdentityQuery identityQuery, CancellationToken cancellationToken)
    {
        if (await identityQuery.CountAsync(new Query.Identity.Specification().WithName(message.AdministratorIdentityName), cancellationToken) == 0)
        {
            return Results.BadRequest($"Could not find an identity with name '{message.AdministratorIdentityName}'.");
        }

        await bus.SendAsync(sessionContext.Audit(new Messages.v1.RegisterTenant
        {
            Id = message.Id ?? Guid.NewGuid(),
            Name = message.Name,
            Status = message.Status,
            LogoUrl = message.LogoUrl,
            LogoSvg = message.LogoSvg
        }), cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> Search([FromBody] Contracts.v1.Tenant.Specification specification, ITenantQuery tenantQuery)
    {
        var search = new Query.Tenant.Specification();

        if (!string.IsNullOrWhiteSpace(specification.NameMatch))
        {
            search.WithNameMatch(specification.NameMatch);
        }

        return Results.Ok((await tenantQuery.SearchAsync(search)).Select(Map).ToList());
    }
}