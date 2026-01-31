using Moq;
using NUnit.Framework;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;

namespace Shuttle.Access.Tests;

[TestFixture]
public class SessionServiceFixture
{
    [Test]
    public async Task Should_be_able_check_for_non_existent_session_async()
    {
        var repository = new Mock<ISessionRepository>();

        repository.Setup(m => m.SearchAsync(It.IsAny<SessionSpecification>(), CancellationToken.None)).ReturnsAsync([]);

        var service = new SessionService(new NullSessionCache(), new HashingService(), new Mock<ISessionRepository>().Object);

        Assert.That(await service.FindAsync(new()), Is.Null);
    }

    [Test]
    public async Task Should_be_able_check_for_and_cache_existent_session_async()
    {
        var sessionRepository = new Mock<ISessionRepository>();

        var sessionToken = Guid.NewGuid();
        Session session = new(Guid.NewGuid(), sessionToken.ToByteArray(), sessionToken, "test-user", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        sessionRepository.Setup(m => m.SearchAsync(It.IsAny<SessionSpecification>(), CancellationToken.None)).ReturnsAsync([session]);

        var service = new SessionService(new NullSessionCache(), new HashingService(), sessionRepository.Object);

        Assert.That(await service.FindAsync(new()), Is.Not.Null);

        sessionRepository.Verify(m => m.SearchAsync(It.IsAny<SessionSpecification>(), CancellationToken.None), Times.Exactly(1));
    }
}