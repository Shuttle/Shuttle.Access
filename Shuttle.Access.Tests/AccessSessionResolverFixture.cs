using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Query;
using Shuttle.Access.WebApi;
using Shuttle.Mediator;

namespace Shuttle.Access.Tests;

[TestFixture]
public class AccessSessionResolverFixture
{
    private static DefaultHttpContext GetHttpContext(Guid sessionToken)
    {
        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers.Authorization = $"Shuttle.Access token={sessionToken:D}";

        return httpContext;
    }

    private static DefaultHttpContext GetBearerHttpContext(string token)
    {
        var httpContext = new DefaultHttpContext();

        httpContext.Request.Headers.Authorization = $"Bearer {token}";

        return httpContext;
    }

    private static AccessSessionResolver GetResolver(ISessionCache sessionCache, ISessionQuery sessionQuery, IJwtService? jwtService = null, IMediator? mediator = null)
    {
        return new(Options.Create(new AccessOptions()), sessionCache, sessionQuery, jwtService ?? new Mock<IJwtService>().Object, mediator ?? new Mock<IMediator>().Object);
    }

    [Test]
    public async Task Should_not_resolve_a_session_for_an_unknown_token_async()
    {
        var sessionQuery = new Mock<ISessionQuery>();

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<Session.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await GetResolver(new NullSessionCache(), sessionQuery.Object).ResolveAsync(GetHttpContext(Guid.NewGuid()));

        Assert.That(result.IsAuthenticated, Is.False);
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task Should_resolve_and_cache_a_session_for_a_known_token_async()
    {
        var hashingService = new HashingService();
        var sessionQuery = new Mock<ISessionQuery>();
        var sessionToken = Guid.NewGuid();

        Session session = new()
        {
            Id = Guid.NewGuid(),
            IdentityId = Guid.NewGuid(),
            IdentityName = "test-user",
            DateRegistered = DateTimeOffset.UtcNow,
            ExpiryDate = DateTimeOffset.UtcNow.AddHours(1),
            Tokens =
            [
                new()
                {
                    TokenHash = Convert.ToHexString(hashingService.Sha256($"{sessionToken:D}"))
                }
            ]
        };

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<Session.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([session]);

        var resolver = GetResolver(new SessionCache(hashingService), sessionQuery.Object);

        var result = await resolver.ResolveAsync(GetHttpContext(sessionToken));

        Assert.That(result.IsAuthenticated, Is.True);
        Assert.That(result.Session, Is.Not.Null);
        Assert.That(result.SessionToken, Is.EqualTo(sessionToken));

        // The second call has to be served from the cache.
        Assert.That((await resolver.ResolveAsync(GetHttpContext(sessionToken))).IsAuthenticated, Is.True);

        sessionQuery.Verify(m => m.SearchAsync(It.IsAny<Session.Specification>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
    }

    [Test]
    public async Task Should_not_resolve_a_session_when_no_authorization_header_is_present_async()
    {
        var result = await GetResolver(new NullSessionCache(), new Mock<ISessionQuery>().Object).ResolveAsync(new DefaultHttpContext());

        Assert.That(result.IsAuthenticated, Is.False);
        Assert.That(result.IsFailure, Is.False);
    }

    [Test]
    public async Task Should_register_a_session_for_a_bearer_token_identity_with_no_active_session_async()
    {
        const string identityName = "test-user";

        var sessionQuery = new Mock<ISessionQuery>();

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<Session.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var jwtService = new Mock<IJwtService>();

        jwtService.Setup(m => m.GetIdentityNameAsync(It.IsAny<string>())).ReturnsAsync(identityName);
        jwtService.Setup(m => m.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(new TokenValidationResult { IsValid = true });

        var registeredSession = new Session
        {
            Id = Guid.NewGuid(),
            IdentityId = Guid.NewGuid(),
            IdentityName = identityName,
            DateRegistered = DateTimeOffset.UtcNow,
            ExpiryDate = DateTimeOffset.UtcNow.AddHours(1)
        };

        var mediator = new Mock<IMediator>();

        mediator.Setup(m => m.SendAsync(It.IsAny<SessionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((message, _) => ((SessionRequest)message).Registered(Guid.NewGuid(), registeredSession))
            .Returns(Task.CompletedTask);

        var resolver = GetResolver(new NullSessionCache(), sessionQuery.Object, jwtService.Object, mediator.Object);

        var result = await resolver.ResolveAsync(GetBearerHttpContext("jwt"));

        Assert.That(result.IsAuthenticated, Is.True);
        Assert.That(result.Session, Is.SameAs(registeredSession));

        mediator.Verify(m => m.SendAsync(It.Is<SessionRequest>(r => r.IdentityName == identityName && r.RequestType == SessionRequestType.Direct), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_authenticate_a_bearer_token_identity_without_a_session_when_registration_yields_no_session_async()
    {
        const string identityName = "unknown-user";

        var sessionQuery = new Mock<ISessionQuery>();

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<Session.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var jwtService = new Mock<IJwtService>();

        jwtService.Setup(m => m.GetIdentityNameAsync(It.IsAny<string>())).ReturnsAsync(identityName);
        jwtService.Setup(m => m.ValidateTokenAsync(It.IsAny<string>())).ReturnsAsync(new TokenValidationResult { IsValid = true });

        var mediator = new Mock<IMediator>();

        mediator.Setup(m => m.SendAsync(It.IsAny<SessionRequest>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((message, _) => ((SessionRequest)message).UnknownIdentity())
            .Returns(Task.CompletedTask);

        var resolver = GetResolver(new NullSessionCache(), sessionQuery.Object, jwtService.Object, mediator.Object);

        var result = await resolver.ResolveAsync(GetBearerHttpContext("jwt"));

        Assert.That(result.IsAuthenticated, Is.True);
        Assert.That(result.IdentityName, Is.EqualTo(identityName));
        Assert.That(result.Session, Is.Null);
    }
}
