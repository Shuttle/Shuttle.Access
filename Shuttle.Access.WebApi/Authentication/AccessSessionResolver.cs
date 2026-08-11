using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Query;
using Shuttle.Contract;
using Shuttle.Mediator;

namespace Shuttle.Access.WebApi;

/// <summary>
///     Resolves the session directly from the credential presented by the caller — a JSON Web Token validated against
///     the configured <see cref="IssuerOptions" />, or a Shuttle.Access session token.
/// </summary>
/// <remarks>
///     This is the authority, and the only place in the platform where issuers and tokens are validated.  Applications
///     that call this web API use the default `DelegatedSessionResolver` instead, which asks this web API who the
///     caller is, so that security configuration is not duplicated across deployments.
/// </remarks>
public class AccessSessionResolver(IOptions<AccessOptions> accessOptions, ISessionCache sessionCache, ISessionQuery sessionQuery, IJwtService jwtService, IMediator mediator, ILogger<AccessSessionResolver>? logger = null) : ISessionResolver
{
    public const string BearerScheme = "Bearer ";
    public const string SessionTokenScheme = "Shuttle.Access ";

    public static readonly Regex TokenExpression = new(@"token\s*=\s*(?<token>[0-9a-fA-F-]{36})", RegexOptions.IgnoreCase);

    private readonly AccessOptions _accessOptions = Guard.AgainstNull(Guard.AgainstNull(accessOptions).Value);
    private readonly IJwtService _jwtService = Guard.AgainstNull(jwtService);
    private readonly IMediator _mediator = Guard.AgainstNull(mediator);
    private readonly ILogger<AccessSessionResolver> _logger = logger ?? NullLogger<AccessSessionResolver>.Instance;
    private readonly ISessionCache _sessionCache = Guard.AgainstNull(sessionCache);
    private readonly ISessionQuery _sessionQuery = Guard.AgainstNull(sessionQuery);

    public async Task<SessionResolutionResult> ResolveAsync(HttpContext httpContext, CancellationToken cancellationToken = default)
    {
        var header = Guard.AgainstNull(httpContext).Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(header))
        {
            return SessionResolutionResult.None;
        }

        var tenantId = httpContext.Request.GetTenantId(_logger, _accessOptions.SystemTenantId);

        if (!tenantId.HasValue)
        {
            return SessionResolutionResult.Failure(AspNetCore.Resources.InvalidTenantIdHeaderException);
        }

        if (header.StartsWith(BearerScheme, StringComparison.OrdinalIgnoreCase))
        {
            return await ResolveBearerAsync(header[BearerScheme.Length..].Trim(), tenantId.Value, cancellationToken);
        }

        if (header.StartsWith(SessionTokenScheme, StringComparison.OrdinalIgnoreCase))
        {
            return await ResolveSessionTokenAsync(header[SessionTokenScheme.Length..].Trim(), tenantId.Value, cancellationToken);
        }

        AspNetCore.LogMessage.InvalidAuthorizationHeader(_logger, $"The 'Authorization' header does not start with '{BearerScheme.Trim()}' or '{SessionTokenScheme.Trim()}'.");

        return SessionResolutionResult.Failure(Access.Resources.InvalidAuthorizationHeader);
    }

    private async Task<SessionResolutionResult> ResolveBearerAsync(string token, Guid tenantId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            AspNetCore.LogMessage.InvalidAuthorizationHeader(_logger, BearerScheme.Trim());

            return SessionResolutionResult.Failure(Access.Resources.InvalidAuthorizationHeader);
        }

        var identityName = await _jwtService.GetIdentityNameAsync(token);

        if (string.IsNullOrWhiteSpace(identityName))
        {
            AspNetCore.LogMessage.IdentityNameClaimNotFound(_logger);

            return SessionResolutionResult.Failure(Access.Resources.IdentityNameClaimNotFound);
        }

        var tokenValidationResult = await _jwtService.ValidateTokenAsync(token);

        if (!tokenValidationResult.IsValid)
        {
            AspNetCore.LogMessage.InvalidAuthorizationHeader(_logger, BearerScheme.Trim());

            return SessionResolutionResult.Failure(tokenValidationResult.Exception?.Message ?? Access.Resources.InvalidAuthorizationHeader);
        }

        var session = await FindAsync(new Session.Specification().WithIdentityName(identityName), cancellationToken);

        if (session != null)
        {
            return SessionResolutionResult.Authenticated(session, tenantId);
        }

        // The bearer token identifies who the caller is, so a session can — and should — be registered for them
        // directly, rather than requiring a follow-up call to register one.
        var sessionRequest = new SessionRequest(identityName).UseDirect();

        await _mediator.SendAsync(sessionRequest, cancellationToken);

        if (!sessionRequest.HasSession)
        {
            // The identity is valid but no session could be registered for it (e.g. it is not a known
            // Shuttle.Access identity) — the caller is still authenticated, just without a session.
            return SessionResolutionResult.Authenticated(identityName, tenantId);
        }

        _sessionCache.Add(sessionRequest.Session);

        return SessionResolutionResult.Authenticated(sessionRequest.Session, tenantId);
    }

    private async Task<SessionResolutionResult> ResolveSessionTokenAsync(string value, Guid tenantId, CancellationToken cancellationToken)
    {
        var match = TokenExpression.Match(value);

        if (!match.Success ||
            !Guid.TryParse(match.Groups["token"].Value, out var sessionToken))
        {
            AspNetCore.LogMessage.InvalidAuthorizationHeader(_logger, $"The 'token' value '{match.Groups["token"].Value}' provided is not a valid GUID.");

            return SessionResolutionResult.Failure(Access.Resources.InvalidAuthorizationHeader);
        }

        var session = await FindAsync(new Session.Specification().WithToken(sessionToken), cancellationToken);

        return session == null
            ? SessionResolutionResult.Failure(Access.Resources.InvalidAuthorizationHeader)
            : SessionResolutionResult.Authenticated(session, tenantId, sessionToken);
    }

    private async Task<Session?> FindAsync(Session.Specification specification, CancellationToken cancellationToken)
    {
        var session = _sessionCache.Find(specification);

        if (session != null)
        {
            return session;
        }

        session = (await _sessionQuery.SearchAsync(specification, cancellationToken)).FirstOrDefault();

        return session == null ? null : _sessionCache.Add(session);
    }
}
