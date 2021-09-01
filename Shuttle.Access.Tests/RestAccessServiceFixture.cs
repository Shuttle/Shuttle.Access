using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Mvc.Rest;

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
            var accessClient = new Mock<IAccessClient>();

            accessClient.Setup(m => m.GetSession(It.IsAny<Guid>())).Returns(() => null);

            var service = new RestAccessService(new Mock<IAccessConfiguration>().Object, accessClient.Object);

            Assert.That(service.Contains(Guid.NewGuid()), Is.False);
        }

        [Test]
        public void Should_be_able_check_for_and_cache_existent_session()
        {
            var configuration = new Mock<IAccessConfiguration>();
            var accessClient = new Mock<IAccessClient>();

            accessClient.Setup(m => m.GetSession(It.IsAny<Guid>())).Returns(() =>
                GetDataResult<DataAccess.Query.Session>.Success(new DataAccess.Query.Session
                    {Permissions = new List<string>()}));
            configuration.Setup(m => m.SessionDuration).Returns(TimeSpan.FromHours(1));

            var service = new RestAccessService(configuration.Object, accessClient.Object);

            Assert.That(service.Contains(_session.Token), Is.True);
            Assert.That(service.Contains(_session.Token), Is.True);

            accessClient.Verify(m => m.GetSession(It.IsAny<Guid>()), Times.Exactly(1));
        }
    }
}