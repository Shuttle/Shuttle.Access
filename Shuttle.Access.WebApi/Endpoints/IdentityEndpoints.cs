using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Access.WebApi.Contracts.v1;
using Shuttle.Contract;
using Shuttle.Mediator;
using ActivateIdentity = Shuttle.Access.WebApi.Contracts.v1.ActivateIdentity;
using ChangePassword = Shuttle.Access.WebApi.Contracts.v1.ChangePassword;
using RegisterIdentity = Shuttle.Access.WebApi.Contracts.v1.RegisterIdentity;
using ResetPassword = Shuttle.Access.WebApi.Contracts.v1.ResetPassword;

namespace Shuttle.Access.WebApi;

public static class IdentityEndpoints
{
    private static async Task<IResult> Delete(Guid id, ISessionContext sessionContext, MessageDispatcher messageDispatcher)
    {
        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.RemoveIdentity { Id = id }),
            () => new Application.RemoveIdentity(id, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> Get(ISessionContext sessionContext, IIdentityQuery identityQuery, Guid id)
    {
        if (!sessionContext.IsAuthorized)
        {
            return Results.Unauthorized();
        }

        var specification = new Query.Identity.Specification().IncludeTenants().IncludeRoles().IncludePermissions().AddId(id);

        var identity = (await identityQuery.SearchAsync(specification)).SingleOrDefault();

        return identity != null
            ? Results.Ok(identity)
            : Results.BadRequest();
    }

    private static async Task<IResult> GetPasswordResetToken(IMediator mediator, Guid id)
    {
        var message = new GetPasswordResetToken(id);

        await mediator.SendAsync(message);

        return Results.Ok(message.PasswordResetToken);
    }

    private static Contracts.v1.Identity Map(Query.Identity identity)
    {
        return new()
        {
            Id = identity.Id,
            Name = identity.Name,
            Description = identity.Description,
            DateRegistered = identity.DateRegistered,
            DateActivated = identity.DateActivated,
            RegisteredBy = identity.RegisteredBy,
            GeneratedPassword = identity.GeneratedPassword ?? string.Empty,
            Roles = identity.Roles.Select(item => new Contracts.v1.Identity.Role
            {
                Id = item.Id,
                Name = item.Name,
                TenantId = item.TenantId,
                TenantName = item.TenantName
            }).OrderBy(item => item.Name).ThenBy(item => item.TenantName).ToList(),
            Tenants = identity.Tenants.Select(item => new Contracts.v1.Identity.Tenant
            {
                Id = item.Id,
                Name = item.Name
            }).OrderBy(item => item.Name).ToList()
        };
    }

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

        app.MapPost("/v{version:apiVersion}/identities/search", PostSearch)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.View);

        app.MapGet("/v{version:apiVersion}/identities/{id:Guid}", Get)
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

    private static async Task<IResult> PatchActivate([FromBody] ActivateIdentity message, ISessionContext sessionContext, IIdentityQuery identityQuery, MessageDispatcher messageDispatcher)
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

        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.ActivateIdentity
            {
                Id = message.Id,
                Name = message.Name
            }),
            () => new Application.ActivateIdentity(message.Id, message.Name, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchDescription(Guid id, [FromBody] SetDescription message, ISessionContext sessionContext, MessageDispatcher messageDispatcher)
    {
        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.SetIdentityDescription
            {
                Id = id,
                Description = message.Description
            }),
            () => new Application.SetIdentityDescription(id, message.Description, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchName(Guid id, [FromBody] SetName message, ISessionContext sessionContext, MessageDispatcher messageDispatcher)
    {
        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.SetIdentityName
            {
                Id = id,
                Name = message.Name
            }),
            () => new Application.SetIdentityName(id, message.Name, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchPassword([FromBody] ChangePassword message, ISessionContext sessionContext, IMediator mediator)
    {
        if (!sessionContext.IsAuthorized || (message.Id.HasValue && !sessionContext.Session.HasPermission(sessionContext.TenantId, AccessPermissions.Identities.Register)))
        {
            return Results.Unauthorized();
        }

        if (!message.Id.HasValue && !message.Token.HasValue)
        {
            return Results.BadRequest();
        }

        await mediator.SendAsync(message.Id.HasValue
            ? Application.ChangePassword.UseId(message.Id.Value, message.NewPassword, sessionContext.TenantId, sessionContext.Session.IdentityName)
            : Application.ChangePassword.UseToken(message.Token!.Value, message.NewPassword, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchPasswordReset([FromBody] ResetPassword message, ISessionContext sessionContext, IMediator mediator, HttpContext httpContext)
    {
        if (!sessionContext.IsAuthorized)
        {
            return Results.Unauthorized();
        }

        await mediator.SendAsync(new Application.ResetPassword(message.Name, message.Password, message.PasswordResetToken, sessionContext.TenantId, sessionContext.Session.IdentityName));

        return Results.Ok();
    }

    private static async Task<IResult> PatchRoleStatus(Guid id, Guid roleId, [FromBody] SetActiveStatus message, ISessionContext sessionContext, IMediator mediator, IRoleQuery roleQuery, IIdentityQuery identityQuery, MessageDispatcher messageDispatcher, CancellationToken cancellationToken)
    {
        var identity = (await identityQuery.FindAsync(new Query.Identity.Specification().AddId(id).IncludeTenants(), cancellationToken: cancellationToken)).GuardAgainstRecordNotFound(id);

        var role = (await roleQuery.FindAsync(new Query.Role.Specification().AddId(roleId), cancellationToken: cancellationToken)).GuardAgainstRecordNotFound(roleId);

        if (identity.Tenants.All(e => e.Id != role.TenantId))
        {
            return Results.BadRequest($"Identity '{identity.Name}' is not in tenant with id '{role.TenantId}'.");
        }

        if (!message.Active)
        {
            var reviewIdentityRoleRemoval = new ReviewIdentityRoleRemoval(sessionContext.TenantId, roleId);

            await mediator.SendAsync(reviewIdentityRoleRemoval, cancellationToken);

            if (reviewIdentityRoleRemoval.IsLastAdministrator)
            {
                return Results.BadRequest("The user cannot be removed from the administrator role as this is the last administrator.");
            }
        }

        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.SetIdentityRoleStatus
            {
                IdentityId = id,
                RoleId = roleId,
                Active = message.Active
            }),
            () => new Application.SetIdentityRoleStatus(id, roleId, message.Active, sessionContext.TenantId, sessionContext.Session.IdentityName),
            cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> PatchTenantStatus(Guid id, Guid tenantId, [FromBody] SetActiveStatus message, ISessionContext sessionContext, ITenantQuery tenantQuery, IIdentityQuery identityQuery, MessageDispatcher messageDispatcher, CancellationToken cancellationToken)
    {
        if (message.Active)
        {
            var identity = (await identityQuery.FindAsync(new Query.Identity.Specification().AddId(id).IncludeTenants(), cancellationToken: cancellationToken)).GuardAgainstRecordNotFound(id);

            if (identity.Tenants.All(e => e.Id != tenantId))
            {
                var tenant = (await tenantQuery.FindAsync(new Query.Tenant.Specification().AddId(tenantId), cancellationToken: cancellationToken)).GuardAgainstRecordNotFound(tenantId);

                if (tenant.MaximumIdentities > 0 && await identityQuery.CountAsync(new Query.Identity.Specification().WithTenantId(tenantId), cancellationToken) >= tenant.MaximumIdentities)
                {
                    return Results.BadRequest($"Tenant '{tenant.Name}' has reached its maximum number of identities ({tenant.MaximumIdentities}).");
                }
            }
        }

        await messageDispatcher.DispatchAsync(
            () => sessionContext.Audit(new Messages.v1.SetIdentityTenantStatus
            {
                IdentityId = id,
                TenantId = tenantId,
                Active = message.Active
            }),
            () => new Application.SetIdentityTenantStatus(id, tenantId, message.Active, sessionContext.TenantId, sessionContext.Session.IdentityName),
            cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> Post(IOptions<AccessOptions> accessOptions, ISessionContext sessionContext, ITenantQuery tenantQuery, IRoleQuery roleQuery, IIdentityQuery identityQuery, IHashingService hashingService, [FromBody] RegisterIdentity message, MessageDispatcher messageDispatcher, CancellationToken cancellationToken)
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

        var roleIds = new List<Guid>();
        var tenantIds = new List<Guid>();

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

        var identityId = Guid.NewGuid();
        var passwordHash = string.IsNullOrWhiteSpace(message.Password) ? [] : hashingService.Sha256(message.Password);

        await messageDispatcher.DispatchAsync(
            () => new Messages.v1.RegisterIdentity
            {
                Id = identityId,
                Name = message.Name,
                Description = message.Description,
                RegisteredBy = sessionContext.Session.IdentityName,
                AuditTenantId = sessionContext.TenantId,
                AuditIdentityName = sessionContext.Session.IdentityName,
                Activated = true,
                RoleIds = roleIds,
                TenantIds = tenantIds,
                PasswordHash = passwordHash
            },
            () => new Application.RegisterIdentity(identityId, message.Name, message.Description, string.Empty, passwordHash, sessionContext.Session.IdentityName, true, sessionContext.TenantId, sessionContext.Session.IdentityName)
                .AddRoleIds(roleIds)
                .AddTenantIds(tenantIds),
            cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> PostRoleAvailability(ISessionContext sessionContext, IIdentityQuery identityQuery, Guid id, [FromBody] Identifiers<Guid> identifiers)
    {
        if (!sessionContext.IsAuthorized)
        {
            return Results.Unauthorized();
        }

        var roles = (await identityQuery.RoleIdsAsync(new Query.Identity.Specification().AddId(id))).ToList();

        return Results.Ok(from roleId in identifiers.Values select new IdentifierAvailability<Guid> { Id = roleId, Active = roles.Any(item => item.Equals(roleId)) });
    }

    private static async Task<IResult> PostSearch(IIdentityQuery identityQuery, [FromBody] Contracts.v1.Identity.Specification specification)
    {
        var search = new Query.Identity.Specification().AddIds(specification.Ids);

        if (!string.IsNullOrWhiteSpace(specification.Name))
        {
            search.WithName(specification.Name);
        }

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

        return Results.Ok((await identityQuery.SearchAsync(search)).Select(Map).ToList());
    }

    private static async Task<IResult> PostTenantAvailability(Guid id, [FromBody] Identifiers<Guid> identifiers, IIdentityQuery identityQuery)
    {
        var tenants = (await identityQuery.TenantIdsAsync(new Query.Identity.Specification().AddId(id).IncludeTenants())).ToList();

        return Results.Ok(from tenantId in identifiers.Values select new IdentifierAvailability<Guid> { Id = tenantId, Active = tenants.Any(item => item.Equals(tenantId)) });
    }
}