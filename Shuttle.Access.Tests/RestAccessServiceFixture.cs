using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Mvc.DataStore;
using Shuttle.Access.Mvc.Rest;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests
{
    [TestFixture]
    public class RestAccessServiceFixture
    {
        private readonly Session _session = new Session(Guid.NewGuid(), Guid.NewGuid(), "test-user", DateTime.Now,
            DateTime.Now.AddHours(1));

        [Test]
        public void Should_be_able_check_for_non_existent_session()
        {
            var restService = new Mock<IRestService>();

            restService.Setup(m => m.GetPermissions(It.IsAny<Guid>())).Returns(() => null);

            var service = new RestAccessService(new Mock<IAccessConfiguration>().Object, restService.Object);

            Assert.That(service.Contains(Guid.NewGuid()), Is.False);
        }

        [Test]
        public void Should_be_able_check_for_and_cache_existent_session()
        {
            var configuration = new Mock<IAccessConfiguration>();
            var restService = new Mock<IRestService>();

            restService.Setup(m => m.GetPermissions(It.IsAny<Guid>())).Returns(() => new List<string>());
            configuration.Setup(m => m.SessionDuration).Returns(TimeSpan.FromHours(1));

            var service = new RestAccessService(configuration.Object, restService.Object);

            Assert.That(service.Contains(_session.Token), Is.True);
            Assert.That(service.Contains(_session.Token), Is.True);

            restService.Verify(m => m.GetPermissions(It.IsAny<Guid>()), Times.Exactly(1));
        }
    }
}