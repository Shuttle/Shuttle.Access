using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.WebApi.Contracts.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Core.TransactionScope;
using Shuttle.Hopper;

namespace Shuttle.Access.WebApi;

public static class IdentityEndpoints
{
    public static WebApplication MapIdentityEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPatch("/v{version:apiVersion}/identities/{id:Guid}/name", PatchName)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPatch("/v{version:apiVersion}/identities/{id:Guid}/description", PatchDescription)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/identities/search", Search)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.View);

        app.MapGet("/v{version:apiVersion}/identities/{value}", Get)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.View);

        app.MapDelete("/v{version:apiVersion}/identities/{id:Guid}", Delete)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Remove);

        app.MapPatch("/v{version:apiVersion}/identities/{id:Guid}/roles/{roleId:Guid}/status", PatchRoleStatus)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPost("/v{version:apiVersion}/identities/{id:Guid}/roles/availability", PostRoleAvailability)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPatch("/v{version:apiVersion}/identities/{id:Guid}/tenants/{tenantId:Guid}/status", PatchTenantStatus)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPost("/v{version:apiVersion}/identities/{id:Guid}/tenants/availability", PostTenantAvailability)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPatch("/v{version:apiVersion}/identities/password", PatchPassword)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequireSession();

        app.MapPatch("/v{version:apiVersion}/identities/password/reset", PatchPasswordReset)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPatch("/v{version:apiVersion}/identities/activate", PatchActivate)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapGet("/v{version:apiVersion}/identities/{id:Guid}/password/reset-token", GetPasswordResetToken)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPost("/v{version:apiVersion}/identities/", Post)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        return app;
    }

    private static async Task<IResult> PostRoleAvailability(Guid id, [FromBody] Identifiers<Guid> identifiers, IIdentityQuery identityQuery)
    {
        var roles = (await identityQuery.RoleIdsAsync(new Query.Identity.Specification().AddId(id))).ToList();

        return Results.Ok(from roleId in identifiers.Values select new IdentifierAvailability<Guid> { Id = roleId, Active = roles.Any(item => item.Equals(roleId)) });
    }

    private static async Task<IResult> PostTenantAvailability(Guid id, [FromBody] Identifiers<Guid> identifiers, IIdentityQuery identityQuery)
    {
        var tenants = (await identityQuery.TenantIdsAsync(new Query.Identity.Specification().AddId(id))).ToList();

        return Results.Ok(from tenantId in identifiers.Values select new IdentifierAvailability<Guid> { Id = tenantId, Active = tenants.Any(item => item.Equals(tenantId)) });
    }

    private static async Task<IResult> Post(IOptions<AccessOptions> accessOptions, IBus bus, ISessionContext sessionContext, ITenantQuery tenantQuery, IRoleQuery roleQuery, IIdentityQuery identityQuery, ITransactionScopeFactory transactionScopeFactory, [FromBody] Contracts.v1.RegisterIdentity message, CancellationToken cancellationToken)
    {
        Guard.AgainstNull(message);

        if (string.IsNullOrWhiteSpace(message.Name))
        {
            return Results.BadRequest($"'{nameof(message.Name)}' is required.");
        }

        if (!sessionContext.IsAuthorized)
        {
            return Results.Unauthorized();
        }

        var roleIds= new List<Guid>();
        var tenantIds= new List<Guid>();

        if (message.RoleIds.Count > 0 || message.TenantIds.Count > 0)
        {
            var identity = (await identityQuery.SearchAsync(new Query.Identity.Specification().AddId(sessionContext.Session.IdentityId).IncludePermissions(), cancellationToken)).FirstOrDefault();

            if (identity == null)
            {
                return Results.Unauthorized();
            }

            if (message.RoleIds.Count > 0)
            {
                var roles = (await roleQuery.SearchAsync(new Query.Role.Specification().AddIds(message.RoleIds), cancellationToken)).ToList();

                roleIds.AddRange(roles.Select(item => item.Id));
                tenantIds.AddRange(roles.Select(item => item.TenantId).Distinct());
            }

            List<Query.Tenant> tenants = [];

            if (message.TenantIds.Count > 0)
            {
                tenants = (await tenantQuery.SearchAsync(new Query.Tenant.Specification().AddIds(message.TenantIds), cancellationToken)).ToList();

                foreach (var tenantId in tenants.Select(item => item.Id).Distinct())
                {
                    if (!tenantIds.Contains(tenantId))
                    {
                        tenantIds.Add(tenantId);
                    }
                }
            }

            var hasSystemAccess = identity.HasPermission(accessOptions.Value.SystemTenantId, AccessPermissions.Identities.Register);

            if (!hasSystemAccess)
            {
                foreach (var tenantId in tenantIds)
                {
                    if (!identity.HasPermission(tenantId, AccessPermissions.Identities.Register))
                    {
                        return Results.Problem(
                            title: "Forbidden",
                            detail: $"You do not have permission to register identities in tenant '{tenants.FirstOrDefault(item => item.Id == tenantId)?.Name ?? "(unknown)"}'.",
                            statusCode: StatusCodes.Status403Forbidden);
                    }
                }
            }
        }

        await bus.SendAsync(new Messages.v1.RegisterIdentity
        {
            Id = Guid.NewGuid(),
            Name = message.Name,
            Description = message.Description,
            RegisteredBy = sessionContext.Session.IdentityName,
            AuditTenantId = sessionContext.Session.TenantId,
            AuditIdentityName = sessionContext.Session.IdentityName,
            Activated = true,
            RoleIds = roleIds,
            TenantIds = tenantIds
        }, cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> GetPasswordResetToken(IMediator mediator, Guid id)
    {
        var message = new GetPasswordResetToken(id);

        await mediator.SendAsync(message);

        return Results.Ok(message.PasswordResetToken);
    }

    private static async Task<IResult> PatchActivate([FromBody] Contracts.v1.ActivateIdentity message, ISessionContext sessionContext, IBus bus, IIdentityQuery identityQuery)
    {
        var specification = new Query.Identity.Specification();

        if (message.Id.HasValue)
        {
            specification.AddId(message.Id.Value);
        }
        else
        {
            specification.WithName(message.Name);
        }

        var query = (await identityQuery.SearchAsync(specification)).FirstOrDefault();

        if (query == null)
        {
            return Results.BadRequest();
        }

        await bus.SendAsync(sessionContext.Audit(new Messages.v1.ActivateIdentity
        {
            Id = message.Id,
            Name = message.Name
        }));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchPasswordReset([FromBody] Contracts.v1.ResetPassword message, ISessionContext sessionContext, IMediator mediator, HttpContext httpContext)
    {
        if (!sessionContext.IsAuthorized)
        {
            return Results.Unauthorized();
        }

        await mediator.SendAsync(new Application.ResetPassword(message.Name, message.Password, message.PasswordResetToken, sessionContext.Session.TenantId, sessionContext.Session.IdentityName));

        return Results.Ok();
    }

    private static async Task<IResult> PatchPassword([FromBody] Contracts.v1.ChangePassword message, ISessionContext sessionContext, IMediator mediator)
    {
        if (sessionContext.Session == null || message.Id.HasValue && !(sessionContext.Session?.HasPermission(AccessPermissions.Identities.Register) ?? false))
        {
            return Results.Unauthorized();
        }

        if (!message.Id.HasValue && !message.Token.HasValue)
        {
            return Results.BadRequest();
        }

        await mediator.SendAsync(message.Id.HasValue 
        ? Application.ChangePassword.UseId(message.Id.Value, message.NewPassword, sessionContext.Session.TenantId, sessionContext.Session.IdentityName)
        : Application.ChangePassword.UseToken(message.Token!.Value, message.NewPassword, sessionContext.Session.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchRoleStatus(Guid id, Guid roleId, [FromBody] SetActiveStatus message, ISessionContext sessionContext, IMediator mediator, IBus bus)
    {
        if (!message.Active)
        {
            var reviewIdentityRoleRemoval = new ReviewIdentityRoleRemoval(sessionContext.Session!.TenantId, roleId);

            await mediator.SendAsync(reviewIdentityRoleRemoval);

            if (reviewIdentityRoleRemoval.IsLastAdministrator)
            {
                return Results.BadRequest("The user cannot be removed from the administrator role as this is the last administrator.");
            }
        }

        await bus.SendAsync(sessionContext.Audit(new SetIdentityRoleStatus
        {
            IdentityId = id,
            RoleId = roleId,
            Active = message.Active,
        }));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchTenantStatus(Guid id, Guid tenantId, [FromBody] SetActiveStatus message, ISessionContext sessionContext, IMediator mediator, IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new SetIdentityTenantStatus
        {
            IdentityId = id,
            TenantId = tenantId,
            Active = message.Active
        }));

        return Results.Accepted();
    }

    private static async Task<IResult> Delete(Guid id, ISessionContext sessionContext, IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new RemoveIdentity { Id = id }));

        return Results.Accepted();
    }

    private static async Task<IResult> Get(IIdentityQuery identityQuery, string value)
    {
        var specification = new Query.Identity.Specification().IncludeRoles();

        if (Guid.TryParse(value, out var id))
        {
            specification.AddId(id);
        }
        else
        {
            specification.WithName(value);
        }

        var identity = (await identityQuery.SearchAsync(specification)).SingleOrDefault();

        return identity != null
            ? Results.Ok(identity)
            : Results.BadRequest();
    }

    private static async Task<IResult> Search(IIdentityQuery identityQuery, [FromBody] Contracts.v1.Identity.Specification specification)
    {
        var search = new Query.Identity.Specification().AddIds(specification.Ids);

        if (!string.IsNullOrWhiteSpace(specification.NameMatch))
        {
            search.WithNameMatch(specification.NameMatch);
        }

        if (specification.ShouldIncludePermissions)
        {
            search.IncludePermissions();
        }

        if (specification.ShouldIncludeRoles)
        {
            search.IncludeRoles();
        }

        if (specification.ShouldIncludeTenants)
        {
            search.IncludeTenants();
        }

        return Results.Ok((await identityQuery.SearchAsync(search)).ToList());
    }

    private static async Task<IResult> PatchDescription(Guid id, [FromBody] SetDescription message, ISessionContext sessionContext, IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new SetIdentityDescription
        {
            Id = id,
            Description = message.Description
        }));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchName(Guid id, [FromBody] SetName message, ISessionContext sessionContext, IBus bus)
    {
        await bus.SendAsync(sessionContext.Audit(new SetIdentityName
        {
            Id = id,
            Name = message.Name
        }));

        return Results.Accepted();
    }
}