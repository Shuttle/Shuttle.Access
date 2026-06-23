using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shuttle.Access.AspNetCore;
using Shuttle.Access.Query;
using Shuttle.Access.WebApi;
using Shuttle.Mediator;

namespace Shuttle.Access.Tests;

[TestFixture]
public class SessionServiceFixture
{
    [Test]
    public async Task Should_be_able_check_for_non_existent_session_async()
    {
        var sessionQuery = new Mock<ISessionQuery>();

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<Session.Specification>(), CancellationToken.None)).ReturnsAsync([]);

        var service = new SessionService(new Mock<ILogger<SessionService>>().Object, new Mock<IHttpContextAccessor>().Object, new NullSessionCache(), sessionQuery.Object, new Mock<IJwtService>().Object, new Mock<IMediator>().Object);

        Assert.That(await service.FindAsync(new()), Is.Null);
    }

    [Test]
    public async Task Should_be_able_check_for_and_cache_existent_session_async()
    {
        var sessionQuery = new Mock<ISessionQuery>();
        var sessionToken = Guid.NewGuid();

        Session session = new()
        {
            Id = Guid.NewGuid(),
            IdentityName = "test-user",
            DateRegistered = DateTimeOffset.UtcNow,
            ExpiryDate = DateTimeOffset.UtcNow.AddHours(1),
            Tokens =
            [
                new()
                {
                    TokenHash = Convert.ToHexString(sessionToken.ToByteArray())
                }
            ]
        };

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<Session.Specification>(), CancellationToken.None)).ReturnsAsync([session]);

        var service = new SessionService(new Mock<ILogger<SessionService>>().Object, new Mock<IHttpContextAccessor>().Object, new NullSessionCache(), sessionQuery.Object, new Mock<IJwtService>().Object, new Mock<IMediator>().Object);

        Assert.That(await service.FindAsync(new()), Is.Not.Null);

        sessionQuery.Verify(m => m.SearchAsync(It.IsAny<Session.Specification>(), CancellationToken.None), Times.Exactly(1));
    }
}