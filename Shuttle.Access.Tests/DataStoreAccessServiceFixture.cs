using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests;

[TestFixture]
public class DataStoreAccessServiceFixture
{
    [Test]
    public async Task Should_be_able_check_for_non_existent_session_async()
    {
        var repository = new Mock<ISessionRepository>();

        repository.Setup(m => m.FindAsync(It.IsAny<byte[]>(), CancellationToken.None)).Returns(Task.FromResult<Session?>(null));

        var connectionStringOptions = new Mock<IOptionsMonitor<ConnectionStringOptions>>();

        connectionStringOptions.Setup(m => m.Get("Access")).Returns(new ConnectionStringOptions
        {
            Name = "Access",
            ConnectionString = "connection-string"
        });

        var service = new DataStoreSessionCache(Options.Create(new AccessOptions
        {
            SessionDuration = TimeSpan.FromHours(1)
        }), new HashingService(), new Mock<IDatabaseContextFactory>().Object, repository.Object);

        Assert.That(await service.FindAsync(Guid.NewGuid()), Is.Null);
    }

    [Test]
    public async Task Should_be_able_check_for_and_cache_existent_session_async()
    {
        var sessionRepository = new Mock<ISessionRepository>();

        var sessionToken = Guid.NewGuid();
        Session session = new(sessionToken.ToByteArray(), sessionToken, "test-user", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        sessionRepository.Setup(m => m.FindAsync(It.IsAny<byte[]>(), CancellationToken.None)).Returns(Task.FromResult(session)!);

        var connectionStringOptions = new Mock<IOptionsMonitor<ConnectionStringOptions>>();

        connectionStringOptions.Setup(m => m.Get("Access")).Returns(new ConnectionStringOptions
        {
            Name = "Access",
            ConnectionString = "connection-string"
        });

        var service = new DataStoreSessionCache(Options.Create(new AccessOptions
        {
            SessionDuration = TimeSpan.FromHours(1)
        }), new HashingService(), new Mock<IDatabaseContextFactory>().Object, sessionRepository.Object);

        Assert.That(await service.FindByTokenAsync(sessionToken), Is.Not.Null);

        sessionRepository.Verify(m => m.FindAsync(It.IsAny<byte[]>(), CancellationToken.None), Times.Exactly(1));
    }
}