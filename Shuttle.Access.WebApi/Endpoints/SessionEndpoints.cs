using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.WebApi.Endpoints;

public static class SessionEndpoints
{
    public static void MapSessionEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/v{version:apiVersion}/sessions", async (ISessionService sessionService, IDatabaseContextFactory databaseContextFactory, [FromBody] RegisterSession message) =>
            {
                if (string.IsNullOrEmpty(message.IdentityName) ||
                    string.IsNullOrEmpty(message.Password) && Guid.Empty.Equals(message.Token))
                {
                    return Results.BadRequest();
                }

                RegisterSessionResult registerSessionResult;

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    registerSessionResult = await sessionService.RegisterAsync(message.IdentityName, message.Password, message.Token);
                }

                return registerSessionResult.Ok
                    ? Results.Ok(new SessionRegistered
                    {
                        IdentityName = registerSessionResult.IdentityName,
                        Token = registerSessionResult.Token,
                        TokenExpiryDate = registerSessionResult.TokenExpiryDate,
                        Permissions = registerSessionResult.Permissions.ToList()
                    })
                    : Results.BadRequest();
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapPost("/v{version:apiVersion}/sessions/delegated", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionService sessionService, RegisterDelegatedSession message) =>
            {
                if (string.IsNullOrEmpty(message.IdentityName))
                {
                    return Results.BadRequest();
                }

                var sessionTokenResult = httpContext.GetAccessSessionToken();

                if (!sessionTokenResult.Ok)
                {
                    return Results.Unauthorized();
                }

                RegisterSessionResult registerSessionResult;

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    registerSessionResult = await sessionService.RegisterAsync(message.IdentityName, sessionTokenResult.SessionToken);
                }

                if (!registerSessionResult.Ok)
                {
                    return Results.BadRequest();
                }

                return Results.Ok(new SessionRegistered
                {
                    IdentityName = registerSessionResult.IdentityName,
                    Token = registerSessionResult.Token,
                    TokenExpiryDate = registerSessionResult.TokenExpiryDate,
                    Permissions = registerSessionResult.Permissions.ToList()
                });
            })
            .WithTags("Sessions")
            .RequiresSession()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapDelete("/v{version:apiVersion}/sessions", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionService sessionService) =>
            {
                var sessionTokenResult = httpContext.GetAccessSessionToken();

                if (!sessionTokenResult.Ok)
                {
                    return Results.BadRequest();
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    return await sessionService.RemoveAsync(sessionTokenResult.SessionToken)
                        ? Results.Ok()
                        : Results.Problem();
                }
            })
            .WithTags("Sessions")
            .RequiresPermission(Permissions.View.Sessions)
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/sessions/{token:Guid}", async (Guid token, IDatabaseContextFactory databaseContextFactory, ISessionQuery sessionQuery) =>
            {
                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var session = await sessionQuery.GetAsync(token);

                    return session == null
                        ? Results.BadRequest()
                        : Results.Ok(session);
                }
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Sessions);

        app.MapGet("/v{version:apiVersion}/sessions/{token:Guid}/permissions", async (Guid token, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository) =>
            {
                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var session = await sessionRepository.FindAsync(token);

                    return session == null
                        ? Results.BadRequest()
                        : Results.Ok(session.Permissions);
                }
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequiresPermission(Permissions.View.Sessions);
    }
}