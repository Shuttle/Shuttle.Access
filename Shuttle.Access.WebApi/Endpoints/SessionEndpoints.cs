using System.Transactions;
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
using Shuttle.Hopper;
using RegisterSession = Shuttle.Access.Application.RegisterSession;
using Session = Shuttle.Access.Query.Session;

namespace Shuttle.Access.WebApi;

public static class SessionEndpoints
{
    private static async Task<IResult> Delete(Guid sessionId, ISessionContext sessionContext, IBus bus, ISessionQuery sessionQuery, CancellationToken cancellationToken)
    {
        using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var specification = new Session.Specification().AddId(sessionId);
            var session = (await sessionQuery.SearchAsync(specification, cancellationToken)).FirstOrDefault();

            if (session != null && await sessionQuery.RemoveAsync(specification, cancellationToken) > 0)
            {
                await bus.PublishAsync(new SessionDeleted
                {
                    Id = session.Id,
                    IdentityId = session.IdentityId,
                    IdentityName = session.IdentityName,
                    TenantId = session.TenantId
                }, cancellationToken);
            }

            tx.Complete();
        }

        return Results.Ok();
    }

    private static async Task<IResult> DeleteAll(IBus bus, ISessionQuery sessionQuery, CancellationToken cancellationToken)
    {
        using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await sessionQuery.RemoveAsync(new(), cancellationToken);

            await bus.PublishAsync(new AllSessionsDeleted(), cancellationToken);

            tx.Complete();
        }

        return Results.Ok();
    }

    private static async Task<IResult> DeleteSelf(IBus bus, ISessionQuery sessionQuery, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var identityId = httpContext.FindIdentityId();
        var tenantId = httpContext.FindTenantId();

        if (tenantId == null || identityId == null)
        {
            return Results.BadRequest();
        }

        using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var session = (await sessionQuery.SearchAsync(new Session.Specification().WithTenantId(tenantId.Value).WithIdentityId(identityId.Value), cancellationToken)).FirstOrDefault();

            if (session != null)
            {
                if (await sessionQuery.RemoveAsync(new Session.Specification().AddId(session.Id), cancellationToken) > 0)
                {
                    await bus.PublishAsync(new SessionDeleted
                    {
                        Id = session.Id,
                        IdentityId = session.IdentityId,
                        IdentityName = session.IdentityName,
                        TenantId = session.TenantId
                    }, cancellationToken);
                }
            }

            tx.Complete();
        }

        return Results.Ok();
    }

    private static async Task<IResult> GetSelf(IOptions<AccessOptions> accessOptions, ISessionContext sessionContext, ISessionCache sessionCache, ISessionQuery sessionQuery, IMediator mediator, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var identityName = httpContext.FindIdentityName();
        var token = httpContext.FindToken();

        if (string.IsNullOrWhiteSpace(identityName))
        {
            return Results.BadRequest();
        }

        var registerSession = new RegisterSession(identityName);

        if (token.HasValue)
        {
            registerSession.UseSessionToken(token.Value);
        }
        else
        {
            registerSession.UseDirect();
        }

        var tenantId = httpContext.FindTenantId();

        if (tenantId.HasValue)
        {
            registerSession.WithTenantId(tenantId.Value);
        }

        await mediator.SendAsync(registerSession, cancellationToken);

        if (registerSession.Result == SessionRegistrationResult.Forbidden)
        {
            return Results.Forbid();
        }

        return !registerSession.HasSession ? Results.NotFound() : Results.Ok(registerSession.Session.Map());
    }

    private static Session.Specification GetSpecification(Contracts.v1.Session.Specification model, IHashingService hashingService)
    {
        var specification = new Session.Specification();

        if (model.Token != null)
        {
            specification.WithTokenHash(hashingService.Sha256(model.Token.Value.ToString("D")));
        }

        if (model.TokenHash != null)
        {
            specification.WithTokenHash(model.TokenHash);
        }

        if (model.IdentityId.HasValue)
        {
            specification.WithIdentityId(model.IdentityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(model.IdentityName))
        {
            specification.WithIdentityName(model.IdentityName);
        }

        if (!string.IsNullOrWhiteSpace(model.IdentityNameMatch))
        {
            specification.WithIdentityNameMatch(model.IdentityNameMatch);
        }

        return specification;
    }

    public static WebApplication MapSessionEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/v{version:apiVersion}/sessions/search", PostSearch)
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Sessions.View)
            .Produces<List<Contracts.v1.Session>>();

        app.MapPost("/v{version:apiVersion}/sessions", Post)
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapPatch("/v{version:apiVersion}/sessions/tenant", PatchTenant)
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapPost("/v{version:apiVersion}/sessions/delegated", PostDelegated)
            .WithTags("Sessions")
            .RequireSession()
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapDelete("/v{version:apiVersion}/sessions/self", DeleteSelf)
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapGet("/v{version:apiVersion}/sessions/self", GetSelf)
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapDelete("/v{version:apiVersion}/sessions", DeleteAll)
            .WithTags("Sessions")
            .RequirePermission(AccessPermissions.Sessions.Manage)
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        app.MapDelete("/v{version:apiVersion}/sessions/{identityId:Guid}", Delete)
            .WithTags("Sessions")
            .RequirePermission(AccessPermissions.Sessions.Manage)
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static async Task<IResult> PatchTenant(ISessionContext sessionContext, SelectTenant selectTenant, IMediator mediator, CancellationToken cancellationToken)
    {
        if (sessionContext.Session == null)
        {
            return Results.BadRequest();
        }

        var message = new SessionTenantSelected(sessionContext.Session.Id, selectTenant.TenantId);

        await Guard.AgainstNull(mediator).SendAsync(message, cancellationToken);

        if (message.Session == null)
        {
            return Results.BadRequest();
        }

        return Results.Ok(message.Session.Map());
    }

    private static async Task<IResult> Post(ILogger<RegisterSession> logger, IOptions<ApiOptions> apiOptions, IBus bus, ISessionContext sessionContext, IMediator mediator, HttpContext httpContext, [FromBody] Contracts.v1.RegisterSession message, CancellationToken cancellationToken)
    {
        var options = Guard.AgainstNull(apiOptions.Value);

        if (!options.AllowPasswordAuthentication && !string.IsNullOrWhiteSpace(message.Password))
        {
            return Results.BadRequest(Resources.PasswordAuthenticationNotAllowed);
        }

        if (string.IsNullOrWhiteSpace(message.IdentityName))
        {
            return Results.BadRequest(Resources.SessionIdentityNameRequired);
        }

        var registerSession = new RegisterSession(message.IdentityName);

        if (message.TenantId.HasValue)
        {
            registerSession.WithTenantId(message.TenantId.Value);
        }

        if (!string.IsNullOrWhiteSpace(message.Password))
        {
            registerSession.Refresh().UsePassword(message.Password);
        }
        else if (!Guid.Empty.Equals(message.Token))
        {
            registerSession.UseSessionToken(message.Token);
        }
        else
        {
            var tenantId = httpContext.FindTenantId();
            var identityName = httpContext.FindIdentityName();

            if (string.IsNullOrWhiteSpace(identityName) || !identityName.Equals(message.IdentityName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (sessionContext.Session == null)
                {
                    if (tenantId == null || string.IsNullOrWhiteSpace(identityName))
                    {
                        return Results.BadRequest(Resources.HttpContextIdentityNotFound);
                    }

                    LogMessage.RegisterSessionIdentityMismatch(logger, identityName, message.IdentityName);

                    return Results.BadRequest($"The identity determined from the HTTP Context is '{identityName}' but the session registration request is for '{message.IdentityName}'.");
                }

                if (!sessionContext.Session.HasPermission(AccessPermissions.Sessions.Register))
                {
                    LogMessage.RegisterSessionUnauthorized(logger, sessionContext.Session.IdentityName);

                    return Results.Unauthorized();
                }
            }

            registerSession.UseDirect();
        }

        await mediator.SendAsync(registerSession, cancellationToken);

        foreach (var session in registerSession.SessionsRemoved)
        {
            await bus.PublishAsync(new SessionDeleted
            {
                Id = session.Id,
                IdentityId = session.IdentityId,
                IdentityName = session.IdentityName,
                TenantId = session.TenantId
            }, cancellationToken);
        }

        return Results.Ok(registerSession.GetSessionResponse(false));
    }

    private static async Task<IResult> PostDelegated(IMediator mediator, RegisterDelegatedSession message, HttpContext httpContext)
    {
        if (string.IsNullOrEmpty(message.IdentityName))
        {
            return Results.BadRequest();
        }

        var sessionIdentityId = httpContext.FindIdentityId();

        if (sessionIdentityId == null)
        {
            return Results.Unauthorized();
        }

        var registerSession = new RegisterSession(message.IdentityName).UseDelegation(message.TenantId, sessionIdentityId.Value);

        await mediator.SendAsync(registerSession);

        return Results.Ok(registerSession.GetSessionResponse(false));
    }

    private static async Task<IResult> PostSearch([FromBody] Contracts.v1.Session.Specification model, ISessionQuery sessionQuery, IHashingService hashingService)
    {
        var specification = GetSpecification(model, hashingService);

        return Results.Ok((await sessionQuery.SearchAsync(specification)).Select(session => session.Map()).ToList());
    }
}