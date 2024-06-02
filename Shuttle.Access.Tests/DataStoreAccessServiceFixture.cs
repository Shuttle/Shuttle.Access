using System;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Mvc.DataStore;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests
{
    [TestFixture]
    public class DataStoreAccessServiceFixture
    {
        private readonly Session _session = new(Guid.NewGuid(), Guid.NewGuid(), "test-user", DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1));

        [Test]
        public void Should_be_able_check_for_non_existent_session()
        {
            var repository = new Mock<ISessionRepository>();

            repository.Setup(m => m.FindAsync(It.IsAny<Guid>())).Returns(() => null);

            var connectionStringOptions = new Mock<IOptionsMonitor<ConnectionStringOptions>>();

            connectionStringOptions.Setup(m => m.Get("Access")).Returns(new ConnectionStringOptions
            {
                Name = "Access",
                ConnectionString = "connection-string"
            });

            var service = new DataStoreAccessService(connectionStringOptions.Object, Options.Create(new AccessOptions
                {
                    SessionDuration = TimeSpan.FromHours(1)
                }), new Mock<IDatabaseContextFactory>().Object, repository.Object);

            Assert.That(service.Contains(Guid.NewGuid()), Is.False);
        }

        [Test]
        public void Should_be_able_check_for_and_cache_existent_session()
        {
            var repository = new Mock<ISessionRepository>();

            repository.Setup(m => m.FindAsync(It.IsAny<Guid>())).Returns(() => _session);

            var connectionStringOptions = new Mock<IOptionsMonitor<ConnectionStringOptions>>();

            connectionStringOptions.Setup(m => m.Get("Access")).Returns(new ConnectionStringOptions
            {
                Name = "Access",
                ConnectionString = "connection-string"
            });

            var service = new DataStoreAccessService(connectionStringOptions.Object, Options.Create(new AccessOptions
                {
                    SessionDuration = TimeSpan.FromHours(1)
                }), new Mock<IDatabaseContextFactory>().Object, repository.Object);

            Assert.That(service.Contains(_session.Token), Is.True);
            Assert.That(service.Contains(_session.Token), Is.True);

            repository.Verify(m => m.FindAsync(It.IsAny<Guid>()), Times.Exactly(1));
        }
    }
}