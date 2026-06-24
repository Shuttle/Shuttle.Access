using Microsoft.AspNetCore.Mvc;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Query;
using Shuttle.Contract;
using Shuttle.Mediator;

namespace Shuttle.Access.WebApi;

public class SessionService(ILogger<SessionService> logger, IHttpContextAccessor httpContextAccessor, ISessionCache sessionCache, ISessionQuery sessionQuery, IJwtService jwtService, IMediator mediator)
    : ISessionService
{
    private const string Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2";

    public async Task<Session?> GetSelfAsync(CancellationToken cancellationToken = default)
    {
        var httpContext = Guard.AgainstNull(httpContextAccessor.HttpContext);

        var authorizationHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();

        if (authorizationHeader == null)
        {
            return null;
        }

        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
            authorizationHeader.Length < 8)
        {
            LogMessage.InvalidAuthorizationHeader(logger, "Bearer");
            return null;
        }

        var token = authorizationHeader[7..];
        var identityName = await jwtService.GetIdentityNameAsync(token);

        if (string.IsNullOrWhiteSpace(identityName))
        {
            LogMessage.IdentityNameClaimNotFound(logger);
            return null;
        }

        var tokenValidationResult = await jwtService.ValidateTokenAsync(token);

        if (!tokenValidationResult.IsValid)
        {
            LogMessage.InvalidAuthorizationHeader(logger, "Bearer");

            var failureMessage = tokenValidationResult.Exception?.Message ?? Resources.InvalidAuthorizationHeader;

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Type = Type,
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = failureMessage
            }, cancellationToken);

            return null;
        }

        var sessionRequest = new SessionRequest(identityName).UseDirect();

        await mediator.SendAsync(sessionRequest, cancellationToken);

        return sessionRequest.Session;
    }

    public async Task<Session?> FindAsync(Session.Specification specification, CancellationToken cancellationToken = default)
    {
        var session = sessionCache.Find(specification);

        return session ?? Add((await sessionQuery.SearchAsync(specification, cancellationToken)).FirstOrDefault());
    }

    private Session? Add(Session? session)
    {
        return session == null ? null : sessionCache.Add(session);
    }
}