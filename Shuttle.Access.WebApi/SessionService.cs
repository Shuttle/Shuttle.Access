using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Query;
using Shuttle.Mediator;

namespace Shuttle.Access.WebApi;

public class SessionService(ILogger<SessionService> logger, IOptions<AccessOptions> accessOptions, IHttpContextAccessor httpContextAccessor, ISessionCache sessionCache, ISessionQuery sessionQuery, IMediator mediator)
    : ISessionService
{
    public async Task<Session?> GetSelfAsync(CancellationToken cancellationToken = default)
    {
        var identityName = httpContextAccessor.HttpContext?.FindIdentityName();

        if (string.IsNullOrWhiteSpace(identityName))
        {
            Shuttle.Access.AspNetCore.LogMessage.IdentityNameClaimNotFound(logger);
            return null;
        }

        var session = await sessionQuery.FindAsync(new Session.Specification().WithIdentityName(identityName), cancellationToken: cancellationToken);

        if (session != null && !session.HasExpired(accessOptions.Value.SessionRenewalTolerance, "Access"))
        {
            return session;
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