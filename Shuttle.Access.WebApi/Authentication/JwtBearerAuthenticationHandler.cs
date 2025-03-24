using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Shuttle.Access.Application;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Mediator;
using Shuttle.OAuth;

namespace Shuttle.Access.WebApi;

public class JwtBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ISessionCache _sessionCache;
    private readonly IMediator _mediator;
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IJwtService _jwtService;
    public static readonly string AuthenticationScheme = "Bearer";
    private const string Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2";

    public JwtBearerAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, IJwtService jwtService, ISessionCache sessionCache, IDatabaseContextFactory databaseContextFactory, IMediator mediator, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
        _jwtService = Guard.AgainstNull(jwtService);
        _sessionCache = Guard.AgainstNull(sessionCache);
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _mediator = Guard.AgainstNull(mediator);
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers["Authorization"].FirstOrDefault();

        if (header == null)
        {
            return AuthenticateResult.NoResult();
        }

        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
            header.Length < 8)
        {
            return AuthenticateResult.NoResult();
        }

        var token = header[7..];
        var tokenValidationResult = await _jwtService.ValidateTokenAsync(token);

        if (!tokenValidationResult.IsValid)
        {
            var failureMessage = (tokenValidationResult.Exception ?? new AuthenticationException(Resources.InvalidAuthenticationHeader)).Message;

            Response.StatusCode = StatusCodes.Status401Unauthorized;

            await Response.WriteAsJsonAsync(new ProblemDetails
            {
                Type = Type,
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = failureMessage
            });

            return AuthenticateResult.Fail(failureMessage);
        }

        var identityName = await _jwtService.GetIdentityNameAsync(token);

        if (string.IsNullOrWhiteSpace(identityName))
        {
            return AuthenticateResult.Fail(Resources.IdentityNameClaimNotFound);
        }

        var session = await _sessionCache.FindAsync(identityName);
        var identityId = session?.IdentityId;

        if (session == null || DateTimeOffset.UtcNow >= session.ExpiryDate)
        {
            using (new DatabaseContextScope())
            await using (_databaseContextFactory.Create())
            {
                var registerSession = new RegisterSession(identityName).UseDirect();

                await _mediator.SendAsync(registerSession);

                if (registerSession.Result != SessionRegistrationResult.Registered)
                {
                    return AuthenticateResult.Fail(registerSession.Result.ToString());
                }

                identityId = registerSession.Session!.IdentityId;

                session = new()
                {
                    IdentityId = registerSession.Session.IdentityId,
                    IdentityName = registerSession.Session.IdentityName,
                    DateRegistered = registerSession.Session.DateRegistered,
                    ExpiryDate = registerSession.Session.ExpiryDate,
                    Permissions = registerSession.Session.Permissions.ToList()
                };
                
                await _sessionCache.AddAsync(registerSession.SessionToken, session);
            }
        }

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, identityName),
            new(ClaimTypes.Name, identityName),
            new(nameof(Session.IdentityName), identityName),
            new(AspNetCore.HttpContextExtensions.SessionIdentityIdClaimType, $"{identityId:D}")
        ];

        return AuthenticateResult.Success(new(new(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var authenticateResult = await HandleAuthenticateOnceAsync();

        if (authenticateResult.Succeeded)
        {
            return;
        }
        
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.Headers.WWWAuthenticate = "Shuttle.Access";

        var problemDetails = new ProblemDetails
        {
            Type = Type,
            Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized),
            Status = StatusCodes.Status401Unauthorized,
            Detail = authenticateResult.Failure?.Message
        };

        await Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null, "application/problem+json");
    }
}