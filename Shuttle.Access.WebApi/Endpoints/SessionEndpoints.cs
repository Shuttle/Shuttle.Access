using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Contract;
using Shuttle.Hopper;
using Shuttle.Mediator;
using System.Transactions;
using Session = Shuttle.Access.Query.Session;

namespace Shuttle.Access.WebApi;

public static class SessionEndpoints
{
    private static async Task<IResult> Delete(ISessionContext sessionContext, IBus bus, ISessionQuery sessionQuery, Guid sessionId, CancellationToken cancellationToken)
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
                    IdentityName = session.IdentityName
                }, cancellationToken);
            }

            tx.Complete();
        }

        return Results.Ok();
    }

    private static async Task<IResult> DeleteAll(ISessionContext sessionContext, IBus bus, ISessionQuery sessionQuery, CancellationToken cancellationToken)
    {
        if (!sessionContext.IsAuthorized)
        {
            return Results.Unauthorized();
        }

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
        var sessionId = httpContext.FindSessionsId();
        var tenantId = httpContext.FindTenantId();

        if (tenantId == null || sessionId == null)
        {
            return Results.BadRequest();
        }

        using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var session = (await sessionQuery.SearchAsync(new Session.Specification().AddId(sessionId.Value), cancellationToken)).FirstOrDefault();

            if (session != null)
            {
                if (await sessionQuery.RemoveAsync(new Session.Specification().AddId(session.Id), cancellationToken) > 0)
                {
                    await bus.PublishAsync(new SessionDeleted
                    {
                        Id = session.Id,
                        IdentityId = session.IdentityId,
                        IdentityName = session.IdentityName
                    }, cancellationToken);
                }
            }

            tx.Complete();
        }

        return Results.Ok();
    }

    private static async Task<IResult> GetSelf(IOptions<AccessOptions> accessOptions, IHashingService hashingService, ISessionContext sessionContext, ISessionCache sessionCache, ISessionQuery sessionQuery, IMediator mediator, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var identityName = httpContext.FindIdentityName();
        var token = httpContext.FindToken();

        if (string.IsNullOrWhiteSpace(identityName))
        {
            return Results.BadRequest();
        }

        var session = await sessionQuery.FindAsync(new Session.Specification().WithIdentityName(identityName), cancellationToken: cancellationToken);

        if (session != null)
        {
            if (token.HasValue)
            {
                var tokenHash = Convert.ToHexString(hashingService.Sha256(token.Value.ToString("D")));
                var sessionToken = session.Tokens.FirstOrDefault(item => item.TokenHash.Equals(tokenHash, StringComparison.InvariantCultureIgnoreCase));

                if (sessionToken != null && !session.HasExpired(accessOptions.Value.SessionRenewalTolerance, sessionToken.Application))
                {
                    return Results.Ok(session.Map());
                }
            }

            if (!session.HasExpired(accessOptions.Value.SessionRenewalTolerance, "Access"))
            {
                return Results.Ok(session.Map());
            }
        }

        var sessionRequest = new SessionRequest(identityName);

        if (token.HasValue)
        {
            sessionRequest.UseSessionToken(token.Value);
        }
        else
        {
            sessionRequest.UseDirect();
        }

        await mediator.SendAsync(sessionRequest, cancellationToken);

        if (sessionRequest.Result == SessionRequestResult.Forbidden)
        {
            return Results.Forbid();
        }

        return !sessionRequest.HasSession ? Results.NotFound() : Results.Ok(sessionRequest.Session.Map());
    }

    private static Session.Specification GetSpecification(Contracts.v1.Session.Specification model, IHashingService hashingService)
    {
        var specification = new Session.Specification();

        if (model.Token != null)
        {
            specification.WithTokenHash(hashingService.Sha256(model.Token.Value.ToString("D")));
        }

        if (!string.IsNullOrWhiteSpace(model.TokenHash))
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

        if (!string.IsNullOrWhiteSpace(model.Application))
        {
            specification.WithApplication(model.Application);
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

        app.MapDelete("/v{version:apiVersion}/sessions/{sessionId:Guid}", Delete)
            .WithTags("Sessions")
            .RequirePermission(AccessPermissions.Sessions.Manage)
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static async Task<IResult> Post(ILogger<SessionRequest> logger, IOptions<AccessOptions> accessOptions, IOptions<ApiOptions> apiOptions, IBus bus, ISessionContext sessionContext, IMediator mediator, HttpContext httpContext, [FromBody] Contracts.v1.SessionRequest message, CancellationToken cancellationToken)
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

        var sessionRequest = new SessionRequest(message.IdentityName);

        if (!string.IsNullOrWhiteSpace(message.Application))
        {
            sessionRequest.WithApplication(message.Application);
        }

        if (!string.IsNullOrWhiteSpace(message.Password))
        {
            sessionRequest.UsePassword(message.Password);
        }
        else if (!Guid.Empty.Equals(message.Token))
        {
            sessionRequest.UseSessionToken(message.Token);
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

                if (!sessionContext.Session.HasPermission(accessOptions.Value.SystemTenantId, AccessPermissions.Sessions.Register))
                {
                    LogMessage.RegisterSessionUnauthorized(logger, sessionContext.Session.IdentityName);

                    return Results.Unauthorized();
                }
            }

            sessionRequest.UseDirect();
        }

        await mediator.SendAsync(sessionRequest, cancellationToken);

        return Results.Ok(sessionRequest.GetSessionResponse(false));
    }

    private static async Task<IResult> PostSearch(ISessionContext sessionContext, ISessionQuery sessionQuery, IHashingService hashingService, [FromBody] Contracts.v1.Session.Specification model)
    {
        if (!sessionContext.IsAuthorized)
        {
            return Results.Unauthorized();
        }

        var specification = GetSpecification(model, hashingService);

        return Results.Ok((await sessionQuery.SearchAsync(specification)).Select(session => session.Map()).ToList());
    }
}