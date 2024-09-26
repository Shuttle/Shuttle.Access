using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.WebApi.Endpoints;

public static class SessionEndpoints
{
    public static WebApplication MapSessionEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/v{version:apiVersion}/sessions", async (IMediator mediator, IDatabaseContextFactory databaseContextFactory, [FromBody] RegisterSession message) =>
            {
                if (string.IsNullOrEmpty(message.IdentityName) ||
                    string.IsNullOrEmpty(message.Password) && Guid.Empty.Equals(message.Token))
                {
                    return Results.BadRequest();
                }

                var registerSession = new Application.RegisterSession(message.IdentityName);

                if (!string.IsNullOrEmpty(message.Password))
                {
                    registerSession.UsePassword(message.Password);
                }
                else
                {
                    registerSession.UseToken(message.Token);
                }

                return await RegisterSession(databaseContextFactory, mediator, registerSession);
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapPost("/v{version:apiVersion}/sessions/delegated", async (HttpContext httpContext, IMediator mediator, IDatabaseContextFactory databaseContextFactory, RegisterDelegatedSession message) =>
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

                var registerSession = new Application.RegisterSession(message.IdentityName).UseDelegation(sessionTokenResult.SessionToken);

                return await RegisterSession(databaseContextFactory, mediator, registerSession);
            })
            .WithTags("Sessions")
            .RequiresSession()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapDelete("/v{version:apiVersion}/sessions", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository) =>
            {
                var sessionTokenResult = httpContext.GetAccessSessionToken();

                if (!sessionTokenResult.Ok)
                {
                    return Results.BadRequest();
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    return await sessionRepository.RemoveAsync(sessionTokenResult.SessionToken)
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

        return app;
    }

    private static async Task<IResult> RegisterSession(IDatabaseContextFactory databaseContextFactory, IMediator mediator, Application.RegisterSession registerSession)
    {
        using (new DatabaseContextScope())
        await using (databaseContextFactory.Create())
        {
            await mediator.SendAsync(registerSession);
        }

        var sessionResponse = new SessionResponse
        {
            Result = registerSession.Result.ToString()
        };

        if (registerSession.HasSession)
        {
            sessionResponse.IdentityName = registerSession.Session.IdentityName;
            sessionResponse.Token = registerSession.Session.Token;
            sessionResponse.TokenExpiryDate = registerSession.Session.ExpiryDate;
            sessionResponse.Permissions = registerSession.Session.Permissions.ToList();
        }

        return Results.Ok(sessionResponse);
    }
}