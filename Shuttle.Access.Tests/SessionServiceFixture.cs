using Moq;
using NUnit.Framework;

namespace Shuttle.Access.Tests;

[TestFixture]
public class SessionServiceFixture
{
    [Test]
    public async Task Should_be_able_check_for_non_existent_session_async()
    {
        var sessionQuery = new Mock<ISessionQuery>();

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Session.Specification>(), CancellationToken.None)).ReturnsAsync([]);

        var service = new SessionService(new NullSessionCache(), sessionQuery.Object);

        Assert.That(await service.FindAsync(new()), Is.Null);
    }

    [Test]
    public async Task Should_be_able_check_for_and_cache_existent_session_async()
    {
        var sessionQuery = new Mock<ISessionQuery>();

        var sessionToken = Guid.NewGuid();
        Query.Session session = new()
        {
            Id = Guid.NewGuid(),
            TokenHash = sessionToken.ToByteArray(),
            IdentityName = "test-user",
            DateRegistered = DateTimeOffset.UtcNow,
            ExpiryDate = DateTimeOffset.UtcNow.AddHours(1)
        };

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Session.Specification>(), CancellationToken.None)).ReturnsAsync([session]);

        var service = new SessionService(new NullSessionCache(), sessionQuery.Object);

        Assert.That(await service.FindAsync(new()), Is.Not.Null);

        sessionQuery.Verify(m => m.SearchAsync(It.IsAny<Query.Session.Specification>(), CancellationToken.None), Times.Exactly(1));
    }
}