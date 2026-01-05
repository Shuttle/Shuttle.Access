using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Access.SqlServer;

namespace Shuttle.Access.Tests;

[TestFixture]
public class SessionServiceFixture
{
    [Test]
    public async Task Should_be_able_check_for_non_existent_session_async()
    {
        var repository = new Mock<ISessionRepository>();

        repository.Setup(m => m.FindAsync(It.IsAny<byte[]>(), CancellationToken.None)).Returns(Task.FromResult<Session?>(null));

        var service = new SessionService(Options.Create(new AccessOptions
        {
            SessionDuration = TimeSpan.FromHours(1)
        }), new HashingService(), new Mock<IAuthorizationService>().Object, new Mock<IIdentityQuery>().Object, repository.Object);

        Assert.That(await service.FindAsync(Guid.NewGuid()), Is.Null);
    }

    [Test]
    public async Task Should_be_able_check_for_and_cache_existent_session_async()
    {
        var sessionRepository = new Mock<ISessionRepository>();

        var sessionToken = Guid.NewGuid();
        Session session = new(Guid.NewGuid(), sessionToken.ToByteArray(), sessionToken, "test-user", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        sessionRepository.Setup(m => m.FindAsync(It.IsAny<byte[]>(), CancellationToken.None)).Returns(Task.FromResult(session)!);

        var service = new SessionService(Options.Create(new AccessOptions
        {
            SessionDuration = TimeSpan.FromHours(1)
        }), new HashingService(), new Mock<IAuthorizationService>().Object, new Mock<IIdentityQuery>().Object, sessionRepository.Object);

        Assert.That(await service.FindByTokenAsync(sessionToken), Is.Not.Null);

        sessionRepository.Verify(m => m.FindAsync(It.IsAny<byte[]>(), CancellationToken.None), Times.Exactly(1));
    }
}