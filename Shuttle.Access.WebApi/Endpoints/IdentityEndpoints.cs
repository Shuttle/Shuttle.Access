using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.WebApi;

public static class IdentityEndpoints
{
    private static Messages.v1.Identity Map(SqlServer.Models.Identity identity)
    {
        return new()
        {
            Id = identity.Id,
            Name = identity.Name,
            Description = identity.Description,
            DateRegistered = identity.DateRegistered,
            DateActivated = identity.DateActivated,
            GeneratedPassword = identity.GeneratedPassword ?? string.Empty,
            RegisteredBy = identity.RegisteredBy,
            Roles = identity.IdentityRoles.Select(item => new Messages.v1.Identity.Role
            {
                Id = item.RoleId,
                Name = item.Role.Name
            }).ToList()
        };
    }

    public static WebApplication MapIdentityEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPatch("/v{version:apiVersion}/identities/{id}/name", PatchName)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPatch("/v{version:apiVersion}/identities/{id:guid}/description", PatchDescription)
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

        app.MapDelete("/v{version:apiVersion}/identities/{id}", Delete)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Remove);

        app.MapPatch("/v{version:apiVersion}/identities/{id}/roles/{roleId}", PatchRole)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPut("/v{version:apiVersion}/identities/password", PutPassword)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapPut("/v{version:apiVersion}/identities/password/reset", PutPasswordReset)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPost("/v{version:apiVersion}/identities/{id}/roles/availability", PostRoleAvailability)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPut("/v{version:apiVersion}/identities/activate", PutActivate)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapGet("/v{version:apiVersion}/identities/{name}/password/reset-token", GetPasswordResetToken)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPost("/v{version:apiVersion}/identities/", Post)
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static async Task<IResult> PostRoleAvailability(IIdentityQuery identityQuery, Guid id, [FromBody] Identifiers<Guid> identifiers)
    {
        try
        {
            identifiers.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        var roles = (await identityQuery.RoleIdsAsync(new SqlServer.Models.Identity.Specification().AddId(id))).ToList();

        return Results.Ok(from roleId in identifiers.Values select new IdentifierAvailability<Guid> { Id = roleId, Active = roles.Any(item => item.Equals(roleId)) });
    }

    private static async Task<IResult> Post(ISessionContext sessionContext, IMediator mediator, [FromBody] RegisterIdentity message)
    {
        Guard.AgainstNull(message);

        try
        {
            message.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        var requestIdentityRegistration = new RequestIdentityRegistration(message);

        if (sessionContext.Session is { TenantId: not null })
        {
            requestIdentityRegistration.Authorized(sessionContext.Session.TenantId.Value, sessionContext.Session.IdentityId);
        }

        await mediator.SendAsync(requestIdentityRegistration);

        return !requestIdentityRegistration.IsAllowed ? Results.Unauthorized() : Results.Accepted();
    }

    private static async Task<IResult> GetPasswordResetToken(IMediator mediator, string name)
    {
        var message = new GetPasswordResetToken { Name = name };

        try
        {
            message.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        var requestResponse = new RequestResponseMessage<GetPasswordResetToken, Guid>(message);
        await mediator.SendAsync(requestResponse);

        return !requestResponse.Ok ? Results.BadRequest(requestResponse.Message) : Results.Ok(requestResponse.Response);
    }

    private static async Task<IResult> PutActivate(IServiceBus serviceBus, IIdentityQuery identityQuery, [FromBody] ActivateIdentity message)
    {
        try
        {
            message.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        var specification = new SqlServer.Models.Identity.Specification();

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

        await serviceBus.SendAsync(message);

        return Results.Accepted();
    }

    private static async Task<IResult> PutPasswordReset(HttpContext httpContext, IMediator mediator, [FromBody] ResetPassword message)
    {
        try
        {
            message.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        var identityId = httpContext.FindIdentityId();

        if (identityId == null)
        {
            return Results.BadRequest(Resources.SessionTokenException);
        }

        var requestMessage = new RequestMessage<ResetPassword>(message);

        await mediator.SendAsync(requestMessage);

        return !requestMessage.Ok
            ? Results.BadRequest(requestMessage.Message)
            : Results.Ok();
    }

    private static async Task<IResult> PutPassword(ISessionContext sessionContext, IMediator mediator, ISessionRepository sessionRepository, [FromBody] ChangePassword message)
    {
        try
        {
            message.ApplyInvariants();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        if (sessionContext.Session is not { TenantId: not null })
        {
            return Results.Unauthorized();
        }

        var session = await sessionRepository.FindAsync(sessionContext.Session.TenantId.Value, sessionContext.Session.IdentityId);

        if (message.Id.HasValue && !(session?.HasPermission(AccessPermissions.Identities.Register) ?? false))
        {
            return Results.Unauthorized();
        }

        var changePassword = new RequestMessage<ChangePassword>(message);

        await mediator.SendAsync(changePassword);

        return !changePassword.Ok
            ? Results.BadRequest(changePassword.Message)
            : Results.Accepted();
    }

    private static async Task<IResult> PatchRole(IMediator mediator, IServiceBus serviceBus, Guid id, Guid roleId, [FromBody] SetIdentityRoleStatus message)
    {
        try
        {
            message.ApplyInvariants();
            message.IdentityId = id;
            message.RoleId = roleId;
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        var reviewRequest = new RequestMessage<SetIdentityRoleStatus>(message);

        await mediator.SendAsync(reviewRequest);

        if (!reviewRequest.Ok)
        {
            return Results.BadRequest(reviewRequest.Message);
        }

        await serviceBus.SendAsync(message);

        return Results.Accepted();
    }

    private static async Task<IResult> Delete(IServiceBus serviceBus, Guid id)
    {
        await serviceBus.SendAsync(new RemoveIdentity { Id = id });

        return Results.Accepted();
    }

    private static async Task<IResult> Get(IIdentityQuery identityQuery, string value)
    {
        var specification = new SqlServer.Models.Identity.Specification().IncludeRoles();

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
            ? Results.Ok(Map(identity))
            : Results.BadRequest();
    }

    private static async Task<IResult> Search(IIdentityQuery identityQuery, [FromBody] Messages.v1.Identity.Specification specification)
    {
        var search = new SqlServer.Models.Identity.Specification();

        if (!string.IsNullOrWhiteSpace(specification.NameMatch))
        {
            search.WithNameMatch(specification.NameMatch);
        }

        if (specification.ShouldIncludeRoles)
        {
            search.IncludeRoles();
        }

        return Results.Ok((await identityQuery.SearchAsync(search)).Select(Map).ToList());
    }

    private static async Task<IResult> PatchDescription(IServiceBus serviceBus, ISessionContext sessionContext, Guid id, [FromBody] SetIdentityDescription message)
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

        await serviceBus.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }

    private static async Task<IResult> PatchName(IServiceBus serviceBus, ISessionContext sessionContext, Guid id, [FromBody] SetIdentityName message)
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

        await serviceBus.SendAsync(sessionContext.Audit(message));

        return Results.Accepted();
    }
}