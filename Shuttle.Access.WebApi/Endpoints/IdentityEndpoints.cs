using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.WebApi.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this WebApplication app, ApiVersionSet versionSet)
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
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Role);

        app.MapGet("/v{version:apiVersion}/identities/", async (IDatabaseContextFactory databaseContextFactory, IIdentityQuery identityQuery) =>
            {
                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    return Results.Ok(await identityQuery.SearchAsync(new DataAccess.Query.Identity.Specification()));
                }
            })
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Identity);

        app.MapGet("/v{version:apiVersion}/identities/{value}", async (IDatabaseContextFactory databaseContextFactory, IIdentityQuery identityQuery, string value) =>
            {
                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var specification = new DataAccess.Query.Identity.Specification().IncludeRoles();

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
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Identity);

        app.MapDelete("/v{version:apiVersion}/identities/{id}", async (IServiceBus serviceBus, Guid id) =>
            {
                await serviceBus.SendAsync(new RemoveIdentity
                {
                    Id = id
                });

                return Results.Accepted();
            })
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Remove.Identity);

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
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Identity);

        app.MapPut("/v{version:apiVersion}/identities/password/change", async (HttpContext httpContext, IMediator mediator, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository, [FromBody] ChangePassword message) =>
            {
                try
                {
                    message.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                var sessionTokenResult = httpContext.GetAccessSessionToken();

                if (!sessionTokenResult.Ok)
                {
                    return Results.BadRequest(Resources.SessionTokenException);
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var session = await sessionRepository.GetAsync(sessionTokenResult.SessionToken);

                    if (message.Id.HasValue && !session.HasPermission(Permissions.Register.Identity))
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

                var sessionTokenResult = httpContext.GetAccessSessionToken();

                if (!sessionTokenResult.Ok)
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
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Identity);

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
                    roles = (await identityQuery.RoleIdsAsync(new DataAccess.Query.Identity.Specification().WithIdentityId(id))).ToList();

                    return Results.Ok(from roleId in identifiers.Values
                        select new IdentifierAvailability<Guid>
                        {
                            Id = roleId,
                            Active = roles.Any(item => item.Equals(roleId))
                        });
                }
            })
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Identity);

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

                var specification = new DataAccess.Query.Identity.Specification();

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
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Identity);

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
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.Register.Identity);

        app.MapPost("/v{version:apiVersion}/identities/", async (HttpContext httpContext, IServiceBus serviceBus, IMediator mediator, IDatabaseContextFactory databaseContextFactory, [FromBody] RegisterIdentity message) =>
            {
                Guard.AgainstNull(message, nameof(message));

                try
                {
                    message.ApplyInvariants();
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(ex.Message);
                }

                var sessionTokenResult = httpContext.GetAccessSessionToken();
                var identityRegistrationRequested = new IdentityRegistrationRequested(sessionTokenResult.Ok ? sessionTokenResult.SessionToken : null);

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    await mediator.SendAsync(identityRegistrationRequested);

                    if (!identityRegistrationRequested.IsAllowed)
                    {
                        return Results.Unauthorized();
                    }

                    if (string.IsNullOrWhiteSpace(message.Password))
                    {
                        var generatePassword = new GeneratePassword();

                        await mediator.SendAsync(generatePassword);

                        message.Password = generatePassword.GeneratedPassword;
                    }

                    var generateHash = new GenerateHash { Value = message.Password };

                    await mediator.SendAsync(generateHash);

                    message.Password = string.Empty;
                    message.PasswordHash = generateHash.Hash;
                    message.RegisteredBy = identityRegistrationRequested.RegisteredBy;
                    message.Activated = message.Activated && sessionTokenResult.Ok &&
                                        identityRegistrationRequested.IsActivationAllowed;
                    message.System = message.System;

                    await serviceBus.SendAsync(message);

                    return Results.Accepted();
                }
            })
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);
    }
}