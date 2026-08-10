using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Query;
using Shuttle.Access.WebApi;

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

    private static AccessSessionResolver GetResolver(ISessionCache sessionCache, ISessionQuery sessionQuery)
    {
        return new(Options.Create(new AccessOptions()), sessionCache, sessionQuery, new Mock<IJwtService>().Object);
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
}
