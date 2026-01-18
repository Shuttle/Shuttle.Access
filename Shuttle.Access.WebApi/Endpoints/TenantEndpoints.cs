using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;

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

        app.MapPost("/v{version:apiVersion}/tenants/", Post)
            .WithTags("Tenants")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Tenants.Register);

        return app;
    }

    private static async Task<IResult> Post(IMediator mediator, [FromBody] RegisterTenant message)
    {
        Guard.AgainstNull(message);

        var requestResponseMessage = new RequestResponseMessage<RegisterTenant, TenantRegistered>(message);

        await mediator.SendAsync(requestResponseMessage);

        return !requestResponseMessage.Ok ? Results.BadRequest(requestResponseMessage.Message) : Results.Ok(requestResponseMessage.Response);
    }

    private static async Task<IResult> Get(ITenantQuery tenantQuery, string value)
    {
        var specification = new SqlServer.Models.Tenant.Specification();

        if (Guid.TryParse(value, out var id))
        {
            specification.AddId(id);
        }
        else
        {
            specification.WithName(value);
        }

        var tenant = (await tenantQuery.SearchAsync(specification)).SingleOrDefault();

        return tenant != null
            ? Results.Ok(Map(tenant))
            : Results.BadRequest();
    }

    private static async Task<IResult> Search(ITenantQuery tenantQuery, [FromBody] Messages.v1.Tenant.Specification specification)
    {
        var search = new SqlServer.Models.Tenant.Specification();

        if (!string.IsNullOrWhiteSpace(specification.NameMatch))
        {
            search.WithNameMatch(specification.NameMatch);
        }

        return Results.Ok((await tenantQuery.SearchAsync(search)).Select(Map).ToList());
    }
}
