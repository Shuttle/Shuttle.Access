using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using RegisterSession = Shuttle.Access.Application.RegisterSession;

namespace Shuttle.Access.WebApi;

public static class SessionEndpoints
{
    public static WebApplication MapSessionEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/v{version:apiVersion}/sessions/search", async (IDatabaseContextFactory databaseContextFactory, ISessionQuery sessionQuery, IHashingService hashingService, [FromBody] Messages.v1.Session.Specification model) =>
            {
                var specification = new DataAccess.Session.Specification();

                if (model.Token != null)
                {
                    specification.WithToken(hashingService.Sha256(model.Token.Value.ToString("D")));
                }

                if (!string.IsNullOrWhiteSpace(model.IdentityName))
                {
                    specification.WithIdentityName(model.IdentityName);
                }

                if (!string.IsNullOrWhiteSpace(model.IdentityNameMatch))
                {
                    specification.WithIdentityNameMatch(model.IdentityNameMatch);
                }

                if (model.ShouldIncludePermissions)
                {
                    specification.IncludePermissions();
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    return Results.Ok((await sessionQuery.SearchAsync(specification)).ToList());
                }
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Sessions.View);

        app.MapPost("/v{version:apiVersion}/sessions", async (IOptions<AccessOptions> accessOptions, IMediator mediator, IDatabaseContextFactory databaseContextFactory, [FromBody] Messages.v1.RegisterSession message) =>
            {
                var options = Guard.AgainstNull(accessOptions.Value);

                if (!options.AllowPasswordAuthentication &&
                    Guid.Empty.Equals(message.Token))
                {
                    return Results.BadRequest(Resources.PasswordAuthenticationNotAllowed);
                }

                if (string.IsNullOrWhiteSpace(message.IdentityName) ||
                    (string.IsNullOrWhiteSpace(message.Password) &&
                     Guid.Empty.Equals(message.Token)))
                {
                    return Results.BadRequest();
                }

                var registerSession = new RegisterSession(message.IdentityName);

                if (!string.IsNullOrWhiteSpace(message.ApplicationName))
                {
                    var knownApplicationOptions = options.KnownApplications.FirstOrDefault(item => item.Name.Equals(message.ApplicationName, StringComparison.InvariantCultureIgnoreCase));

                    if (knownApplicationOptions == null)
                    {
                        return Results.BadRequest($"Unknown application name '{message.ApplicationName}'.");
                    }

                    registerSession.WithKnownApplicationOptions(knownApplicationOptions);
                }

                if (!string.IsNullOrWhiteSpace(message.Password))
                {
                    registerSession.UsePassword(message.Password);
                }
                else
                {
                    registerSession.UseAuthenticationToken(message.Token);
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

                var sessionIdentityId = httpContext.GetIdentityId();

                if (sessionIdentityId == null)
                {
                    return Results.Unauthorized();
                }

                var registerSession = new RegisterSession(message.IdentityName).UseDelegation(sessionIdentityId.Value);

                return await RegisterSession(databaseContextFactory, mediator, registerSession);
            })
            .WithTags("Sessions")
            .RequireSession()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapDelete("/v{version:apiVersion}/sessions/self", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository) =>
            {
                var sessionIdentityId = httpContext.GetIdentityId();

                if (sessionIdentityId == null)
                {
                    return Results.BadRequest();
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    return await sessionRepository.RemoveAsync(sessionIdentityId.Value)
                        ? Results.Ok()
                        : Results.Problem();
                }
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequireSession();

        app.MapGet("/v{version:apiVersion}/sessions/self", async (HttpContext httpContext, IDatabaseContextFactory databaseContextFactory, ISessionQuery sessionQuery) =>
            {
                var sessionIdentityId = httpContext.GetIdentityId();

                if (sessionIdentityId == null)
                {
                    return Results.BadRequest();
                }

                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    return Results.Ok(await sessionQuery.SearchAsync(new DataAccess.Session.Specification().WithIdentityId(sessionIdentityId.Value).IncludePermissions()));
                }
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequireSession();

        app.MapDelete("/v{version:apiVersion}/sessions", async (IDatabaseContextFactory databaseContextFactory, ISessionRepository sessionRepository) =>
            {
                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    await sessionRepository.RemoveAllAsync();
                }

                return Results.Ok();
            })
            .WithTags("Sessions")
            .RequirePermission(AccessPermissions.Sessions.Manage)
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/sessions/exchange/{token:Guid}", async (Guid token, IDatabaseContextFactory databaseContextFactory, ISessionTokenExchangeRepository sessionTokenExchangeRepository, ISessionQuery sessionQuery) =>
            {
                using (new DatabaseContextScope())
                await using (databaseContextFactory.Create())
                {
                    var sessionTokenExchange = await sessionTokenExchangeRepository.FindAsync(token);

                    if (sessionTokenExchange == null)
                    {
                        return Results.BadRequest();
                    }

                    await sessionTokenExchangeRepository.RemoveAsync(token);

                    if (sessionTokenExchange.HasExpired)
                    {
                        return Results.BadRequest();
                    }

                    var session = (await sessionQuery.SearchAsync(new DataAccess.Session.Specification().WithToken(sessionTokenExchange.SessionToken.ToByteArray()).IncludePermissions())).FirstOrDefault();

                    return session == null
                        ? Results.BadRequest()
                        : Results.Ok(session);
                }
            })
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static async Task<IResult> RegisterSession(IDatabaseContextFactory databaseContextFactory, IMediator mediator, RegisterSession registerSession)
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
            sessionResponse.IdentityId = registerSession.Session!.IdentityId;
            sessionResponse.IdentityName = registerSession.Session!.IdentityName;
            sessionResponse.Token = registerSession.SessionToken!.Value;
            sessionResponse.ExpiryDate = registerSession.Session.ExpiryDate;
            sessionResponse.Permissions = registerSession.Session.Permissions.ToList();
            sessionResponse.SessionTokenExchangeUrl = registerSession.SessionTokenExchangeUrl;
            sessionResponse.DateRegistered = registerSession.Session.DateRegistered;
        }

        return Results.Ok(sessionResponse);
    }
}