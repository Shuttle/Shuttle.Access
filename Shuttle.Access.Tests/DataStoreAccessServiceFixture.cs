using System;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Mvc.DataStore;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests
{
    [TestFixture]
    public class DataStoreAccessServiceFixture
    {
        private readonly Session _session = new Session(Guid.NewGuid(), Guid.NewGuid(), "test-user", DateTime.Now,
            DateTime.Now.AddHours(1));

        [Test]
        public void Should_be_able_check_for_non_existent_session()
        {
            var repository = new Mock<ISessionRepository>();

            repository.Setup(m => m.Find(It.IsAny<Guid>())).Returns(() => null);

            var service = new DataStoreAccessService(new Mock<IAccessConfiguration>().Object,
                new Mock<IDatabaseContextFactory>().Object, repository.Object);

            Assert.That(service.Contains(Guid.NewGuid()), Is.False);
        }

        [Test]
        public void Should_be_able_check_for_and_cache_existent_session()
        {
            var repository = new Mock<ISessionRepository>();
            var configuration = new Mock<IAccessConfiguration>();

            repository.Setup(m => m.Find(It.IsAny<Guid>())).Returns(() => _session);
            configuration.Setup(m => m.SessionDuration).Returns(TimeSpan.FromHours(1));

            var service = new DataStoreAccessService(configuration.Object,
                new Mock<IDatabaseContextFactory>().Object, repository.Object);

            Assert.That(service.Contains(_session.Token), Is.True);
            Assert.That(service.Contains(_session.Token), Is.True);

            repository.Verify(m => m.Find(It.IsAny<Guid>()), Times.Exactly(1));
        }
    }
}