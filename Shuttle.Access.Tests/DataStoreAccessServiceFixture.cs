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
    private readonly Session _session = new(Guid.NewGuid(), Guid.NewGuid(), "test-user", DateTime.UtcNow, DateTime.UtcNow.AddHours(1));

    [Test]
    public async Task Should_be_able_check_for_non_existent_session_async()
    {
        var repository = new Mock<ISessionRepository>();

        repository.Setup(m => m.FindAsync(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult<Session?>(null));

        var connectionStringOptions = new Mock<IOptionsMonitor<ConnectionStringOptions>>();

        connectionStringOptions.Setup(m => m.Get("Access")).Returns(new ConnectionStringOptions
        {
            Name = "Access",
            ConnectionString = "connection-string"
        });

        var service = new DataStoreAccessService(Options.Create(new AccessOptions
        {
            SessionDuration = TimeSpan.FromHours(1)
        }), new Mock<IDatabaseContextFactory>().Object, repository.Object);

        Assert.That(await service.ContainsAsync(Guid.NewGuid()), Is.False);
    }

    [Test]
    public async Task Should_be_able_check_for_and_cache_existent_session_async()
    {
        var sessionRepository = new Mock<ISessionRepository>();

        sessionRepository.Setup(m => m.FindAsync(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult(_session)!);

        var connectionStringOptions = new Mock<IOptionsMonitor<ConnectionStringOptions>>();

        connectionStringOptions.Setup(m => m.Get("Access")).Returns(new ConnectionStringOptions
        {
            Name = "Access",
            ConnectionString = "connection-string"
        });

        var service = new DataStoreAccessService(Options.Create(new AccessOptions
        {
            SessionDuration = TimeSpan.FromHours(1)
        }), new Mock<IDatabaseContextFactory>().Object, sessionRepository.Object);

        Assert.That(await service.ContainsAsync(_session.Token), Is.True);

        sessionRepository.Verify(m => m.FindAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Exactly(1));
    }
}