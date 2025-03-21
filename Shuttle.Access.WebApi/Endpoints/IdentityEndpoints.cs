using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi;

public static class IdentityEndpoints
{
    public static WebApplication MapIdentityEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPatch("/v{version:apiVersion}/identities/{id}/name", async (IServiceBus serviceBus, Guid id, [FromBody] SetIdentityName message) =>
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
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Roles.Register);

        app.MapPost("/v{version:apiVersion}/identities/search", async (IDatabaseContextFactory databaseContextFactory, IIdentityQuery identityQuery, [FromBody] Messages.v1.Identity.Specification specification) =>
            {
                var search = new DataAccess.Identity.Specification();

                if (!string.IsNullOrWhiteSpace(specification.NameMatch))
                {
                    search.WithNameMatch(specification.NameMatch);
                }

                if (specification.ShouldIncludeRoles)
                {
                    search.IncludeRoles();
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    return Results.Ok(await identityQuery.SearchAsync(search));
                }
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.View);

        app.MapGet("/v{version:apiVersion}/identities/{value}", async (IDatabaseContextFactory databaseContextFactory, IIdentityQuery identityQuery, string value) =>
            {
                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var specification = new DataAccess.Identity.Specification().IncludeRoles();

                    if (Guid.TryParse(value, out var id))
                    {
                        specification.WithIdentityId(id);
                    }
                    else
                    {
                        specification.WithName(value);
                    }

                    var user = (await identityQuery.SearchAsync(specification)).SingleOrDefault();

                    return user != null
                        ? Results.Ok(user)
                        : Results.BadRequest();
                }
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.View);

        app.MapDelete("/v{version:apiVersion}/identities/{id}", async (IServiceBus serviceBus, Guid id) =>
            {
                await serviceBus.SendAsync(new RemoveIdentity
                {
                    Id = id
                });

                return Results.Accepted();
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Remove);

        app.MapPatch("/v{version:apiVersion}/identities/{id}/roles/{roleId}", async (IMediator mediator, IDatabaseContextFactory databaseContextFactory, IServiceBus serviceBus, Guid id, Guid roleId, [FromBody] SetIdentityRole message) =>
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

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var reviewRequest = new RequestMessage<SetIdentityRole>(message);

                    await mediator.SendAsync(reviewRequest);

                    if (!reviewRequest.Ok)
                    {
                        return Results.BadRequest(reviewRequest.Message);
                    }

                    await serviceBus.SendAsync(message);

                    return Results.Accepted();
                }
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPut("/v{version:apiVersion}/identities/password", async (HttpContext httpContext, IMediator mediator, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository, [FromBody] ChangePassword message) =>
            {
                try
                {
                    message.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                var identityId = httpContext.GetIdentityId();

                if (identityId == null)
                {
                    return Results.Unauthorized();
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var session = await sessionRepository.FindAsync(identityId.Value);

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
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapPut("/v{version:apiVersion}/identities/password/reset", async (HttpContext httpContext, IMediator mediator, IDatabaseContextFactory databaseContextFactory, [FromBody] ResetPassword message) =>
            {
                try
                {
                    message.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                var identityId = httpContext.GetIdentityId();

                if (identityId == null)
                {
                    return Results.BadRequest(Resources.SessionTokenException);
                }

                var requestMessage = new RequestMessage<ResetPassword>(message);

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    await mediator.SendAsync(requestMessage);

                    return !requestMessage.Ok
                        ? Results.BadRequest(requestMessage.Message)
                        : Results.Ok();
                }
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPost("/v{version:apiVersion}/identities/{id}/roles/availability", async (IDatabaseContextFactory databaseContextFactory, IIdentityQuery identityQuery, Guid id, [FromBody] Identifiers<Guid> identifiers) =>
            {
                try
                {
                    identifiers.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                List<Guid> roles;

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    roles = (await identityQuery.RoleIdsAsync(new DataAccess.Identity.Specification().WithIdentityId(id))).ToList();

                    return Results.Ok(from roleId in identifiers.Values
                        select new IdentifierAvailability<Guid>
                        {
                            Id = roleId,
                            Active = roles.Any(item => item.Equals(roleId))
                        });
                }
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPut("/v{version:apiVersion}/identities/activate", async (IServiceBus serviceBus, IDatabaseContextFactory databaseContextFactory, IIdentityQuery identityQuery, [FromBody] ActivateIdentity message) =>
            {
                try
                {
                    message.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                var specification = new DataAccess.Identity.Specification();

                if (message.Id.HasValue)
                {
                    specification.WithIdentityId(message.Id.Value);
                }
                else
                {
                    specification.WithName(message.Name);
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var query = (await identityQuery.SearchAsync(specification)).FirstOrDefault();

                    if (query == null)
                    {
                        return Results.BadRequest();
                    }

                    await serviceBus.SendAsync(message);

                    return Results.Accepted();
                }
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapGet("/v{version:apiVersion}/identities/{name}/password/reset-token", async (IMediator mediator, IDatabaseContextFactory databaseContextFactory, string name) =>
            {
                var message = new GetPasswordResetToken
                {
                    Name = name
                };

                try
                {
                    message.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var requestResponse = new RequestResponseMessage<GetPasswordResetToken, Guid>(message);
                    await mediator.SendAsync(requestResponse);

                    return !requestResponse.Ok ? Results.BadRequest(requestResponse.Message) : Results.Ok(requestResponse.Response);
                }
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Identities.Register);

        app.MapPost("/v{version:apiVersion}/identities/", async (HttpContext httpContext, IMediator mediator, IDatabaseContextFactory databaseContextFactory, [FromBody] RegisterIdentity message) =>
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

                var identityId = httpContext.GetIdentityId();
                var requestIdentityRegistration = new RequestIdentityRegistration(message);

                if (identityId != null)
                {
                    requestIdentityRegistration.WithIdentityId(identityId.Value);
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    await mediator.SendAsync(requestIdentityRegistration);
                }

                return !requestIdentityRegistration.IsAllowed ? Results.Unauthorized() : Results.Accepted();
            })
            .WithTags("Identities")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }
}