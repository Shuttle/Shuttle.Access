using System.Transactions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;
using RegisterSession = Shuttle.Access.Application.RegisterSession;

namespace Shuttle.Access.WebApi;

public static class SessionEndpoints
{
    private static SqlServer.Models.Session.Specification GetSpecification(Messages.v1.Session.Specification model, IHashingService hashingService)
    {
        var specification = new SqlServer.Models.Session.Specification();

        if (model.Token != null)
        {
            specification.WithToken(hashingService.Sha256(model.Token.Value.ToString("D")));
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

        if (model.ShouldIncludePermissions)
        {
            specification.IncludePermissions();
        }

        return specification;
    }

    private static Messages.v1.Session Map(SqlServer.Models.Session session)
    {
        return new()
        {
            TenantId = session.TenantId,
            TenantName = session.Tenant?.Name ?? string.Empty,
            IdentityId = session.IdentityId,
            IdentityName = session.IdentityName,
            IdentityDescription = session.Identity.Description,
            DateRegistered = session.DateRegistered,
            ExpiryDate = session.ExpiryDate,
            Permissions = session.SessionPermissions.Select(item => item.Permission.Name).ToList()
        };
    }

    private static SessionData MapData(SqlServer.Models.Session session)
    {
        return new()
        {
            TenantId = session.TenantId,
            TenantName = session.Tenant?.Name ?? string.Empty,
            IdentityId = session.IdentityId,
            IdentityName = session.IdentityName,
            IdentityDescription = session.Identity.Description,
            DateRegistered = session.DateRegistered,
            ExpiryDate = session.ExpiryDate,
            Permissions = session.SessionPermissions.Select(item => new Messages.v1.Permission
            {
                Id = item.PermissionId,
                Name = item.Permission.Name,
                Description = item.Permission.Description,
                Status = item.Permission.Status
            }).ToList()
        };
    }

    public static WebApplication MapSessionEndpoints(this WebApplication app, ApiVersionSet versionSet)
    {
        var apiVersion1 = new ApiVersion(1, 0);

        app.MapPost("/v{version:apiVersion}/sessions/search", PostSearch)
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Sessions.View)
            .Produces<List<Messages.v1.Session>>();

        app.MapPost("/v{version:apiVersion}/sessions/search/data", PostSearchData)
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1)
            .RequirePermission(AccessPermissions.Sessions.View)
            .Produces<List<SessionData>>();

        app.MapPost("/v{version:apiVersion}/sessions", Post)
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

        app.MapGet("/v{version:apiVersion}/sessions/exchange/{token:Guid}", GetExchange)
            .WithTags("Sessions")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(apiVersion1);

        return app;
    }

    private static async Task<IResult> GetExchange(Guid token, ISessionTokenExchangeRepository sessionTokenExchangeRepository, ISessionQuery sessionQuery)
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

        var session = (await sessionQuery.SearchAsync(new SqlServer.Models.Session.Specification().WithToken(sessionTokenExchange.SessionToken.ToByteArray()).IncludePermissions())).FirstOrDefault();

        return session == null
            ? Results.BadRequest()
            : Results.Ok(Map(session));
    }

    private static async Task<IResult> Delete(Guid sessionId, IServiceBus serviceBus, ISessionRepository sessionRepository, ISessionContext sessionContext)
    {
        using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var session = await sessionRepository.FindAsync(sessionId);

            if (session != null)
            {
                if (await sessionRepository.RemoveAsync(sessionId))
                {
                    await serviceBus.PublishAsync(new SessionDeleted { IdentityId = session.IdentityId, IdentityName = session.IdentityName });
                }
            }

            tx.Complete();
        }

        return Results.Ok();
    }

    private static async Task<IResult> DeleteAll(IServiceBus serviceBus, ISessionRepository sessionRepository)
    {
        using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            await sessionRepository.RemoveAllAsync();

            await serviceBus.PublishAsync(new AllSessionsDeleted());

            tx.Complete();
        }

        return Results.Ok();
    }

    private static async Task<IResult> GetSelf(IOptions<AccessOptions> accessOptions, HttpContext httpContext, ISessionService sessionService, ISessionQuery sessionQuery, IMediator mediator)
    {
        async Task<IResult> AttemptRegistration()
        {
            var identityName = httpContext.FindIdentityName();
            var tenantId = httpContext.FindTenantId();

            if (tenantId == null || string.IsNullOrWhiteSpace(identityName))
            {
                return Results.BadRequest();
            }

            var registerSession = new RegisterSession(identityName).WithTenantId(tenantId.Value).UseDirect();

            await RegisterSession(mediator, registerSession);

            if (registerSession.Result == SessionRegistrationResult.TenantSelectionRequired)
            {
                return Results.Ok(new SessionTenants
                {
                    SessionId = registerSession.Session!.Id,
                    Tenants = registerSession.Tenants.Select(item => new SessionTenants.Tenant
                    {
                        Id = item.Id,
                        Name = item.Name,
                        LogoSvg = item.LogoSvg,
                        LogoUrl = item.LogoUrl
                    }).ToList()
                });
            }
            else if (registerSession.Result != SessionRegistrationResult.Registered || !registerSession.HasSession)
            {
                return Results.NotFound();
            }
            else
            {
                return Results.Ok(new Messages.v1.Session
                {
                    DateRegistered = registerSession.Session!.DateRegistered,
                    ExpiryDate = registerSession.Session.ExpiryDate,
                    IdentityId = registerSession.Session.IdentityId,
                    IdentityName = identityName,
                    Permissions = registerSession.Session.Permissions.Select(item => item.Name).OrderBy(item => item).ToList()
                });
            }
        }

        var sessionIdentityId = httpContext.FindIdentityId();

        if (sessionIdentityId == null)
        {
            return await AttemptRegistration();
        }

        var session = (await sessionQuery.SearchAsync(new SqlServer.Models.Session.Specification().WithIdentityId(sessionIdentityId.Value).IncludePermissions())).FirstOrDefault();

        if (session != null && session.ExpiryDate.Add(accessOptions.Value.SessionRenewalTolerance) > DateTimeOffset.UtcNow)
        {
            return Results.Ok(new Messages.v1.Session
            {
                DateRegistered = session.DateRegistered,
                ExpiryDate = session.ExpiryDate,
                IdentityId = session.IdentityId,
                IdentityName = session.IdentityName,
                Permissions = session.SessionPermissions.Select(item => item.Permission.Name).OrderBy(item => item).ToList()
            });
        }

        await sessionService.FlushAsync(sessionIdentityId.Value);

        return await AttemptRegistration();
    }

    private static async Task<IResult> DeleteSelf(HttpContext httpContext, IServiceBus serviceBus, ISessionRepository sessionRepository)
    {
        var identityId = httpContext.FindIdentityId();
        var tenantId = httpContext.FindTenantId();

        if (tenantId == null || identityId == null)
        {
            return Results.BadRequest();
        }

        using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var session = await sessionRepository.FindAsync(tenantId.Value, identityId.Value);

            if (session != null)
            {
                if (await sessionRepository.RemoveAsync(identityId.Value))
                {
                    await serviceBus.PublishAsync(new SessionDeleted { IdentityId = session.IdentityId, IdentityName = session.IdentityName });
                }
            }

            tx.Complete();
        }

        return Results.Ok();
    }

    private static async Task<IResult> PostDelegated(HttpContext httpContext, IMediator mediator, RegisterDelegatedSession message)
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

        return await RegisterSession(mediator, registerSession);
    }

    private static async Task<IResult> Post(ILogger<RegisterSession> logger, HttpContext httpContext, IOptions<AccessOptions> accessOptions, ISessionContext sessionContext, IMediator mediator, [FromBody] Messages.v1.RegisterSession message)
    {
        var options = Guard.AgainstNull(accessOptions.Value);

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
            registerSession.UsePassword(message.Password);
        }
        else if (!Guid.Empty.Equals(message.Token))
        {
            registerSession.UseAuthenticationToken(message.Token);
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

                    logger.LogDebug($"[BAD REQUEST] : requested identity = '{message.IdentityName}' / http context identity = '{identityName}'");

                    return Results.BadRequest();
                }

                if (!sessionContext.Session.HasPermission(AccessPermissions.Sessions.Register))
                {
                    logger.LogDebug($"[UNAUTHORIZED] : identity id = '{sessionContext.Session.IdentityId}' / permission = '{AccessPermissions.Sessions.Register}'");

                    return Results.Unauthorized();
                }
            }

            registerSession.UseDirect();
        }

        if (!string.IsNullOrWhiteSpace(message.ApplicationName))
        {
            var knownApplicationOptions = options.KnownApplications.FirstOrDefault(item => item.Name.Equals(message.ApplicationName, StringComparison.InvariantCultureIgnoreCase));

            if (knownApplicationOptions == null)
            {
                return Results.BadRequest($"Unknown application name '{message.ApplicationName}'.");
            }

            registerSession.WithKnownApplicationOptions(knownApplicationOptions);
        }

        return await RegisterSession(mediator, registerSession);
    }

    private static async Task<IResult> PostSearchData(ISessionQuery sessionQuery, IHashingService hashingService, [FromBody] Messages.v1.Session.Specification model)
    {
        var specification = GetSpecification(model, hashingService);

        return Results.Ok((await sessionQuery.SearchAsync(specification)).Select(MapData).ToList());
    }

    private static async Task<IResult> PostSearch(ISessionQuery sessionQuery, IHashingService hashingService, [FromBody] Messages.v1.Session.Specification model)
    {
        var specification = GetSpecification(model, hashingService);

        return Results.Ok((await sessionQuery.SearchAsync(specification)).Select(Map).ToList());
    }

    private static async Task<IResult> RegisterSession(IMediator mediator, RegisterSession registerSession)
    {
        await mediator.SendAsync(registerSession);

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
            sessionResponse.Permissions = registerSession.Session.Permissions.Select(item => item.Name).ToList();
            sessionResponse.SessionTokenExchangeUrl = registerSession.SessionTokenExchangeUrl;
            sessionResponse.DateRegistered = registerSession.Session.DateRegistered;
            sessionResponse.TenantId = registerSession.Session.TenantId!.Value;
        }

        return Results.Ok(sessionResponse);
    }
}