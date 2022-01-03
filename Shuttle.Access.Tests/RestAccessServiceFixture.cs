using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Refit;
using Shuttle.Access.Mvc.Rest;
using Shuttle.Access.RestClient;
using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.Tests
{
    [TestFixture]
    public class RestAccessServiceFixture
    {
        private readonly Session _session = new(Guid.NewGuid(), Guid.NewGuid(), "test-user", DateTime.Now,
            DateTime.Now.AddHours(1));

        [Test]
        public void Should_be_able_check_for_non_existent_session()
        {
            var accessClient = new Mock<IAccessClient>();
            var sessionsApi = new Mock<ISessionsApi>();

            sessionsApi.Setup(m => m.Get(It.IsAny<Guid>()).Result).Returns(new ApiResponse<DataAccess.Query.Session>(new HttpResponseMessage(HttpStatusCode.BadRequest), null, new RefitSettings()));
            accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);

            var service = new RestAccessService(new Mock<IAccessConfiguration>().Object, accessClient.Object);

            Assert.That(service.Contains(Guid.NewGuid()), Is.False);
        }

        [Test]
        public void Should_be_able_check_for_and_cache_existing_session()
        {
            var configuration = new Mock<IAccessConfiguration>();
            var accessClient = new Mock<IAccessClient>();
            var sessionsApi = new Mock<ISessionsApi>();

            sessionsApi.Setup(m => m.Get(It.IsAny<Guid>()).Result).Returns(new ApiResponse<DataAccess.Query.Session>(new HttpResponseMessage(HttpStatusCode.OK), new DataAccess.Query.Session { Permissions = new List<string>() }, new RefitSettings()));

            accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);
            configuration.Setup(m => m.SessionDuration).Returns(TimeSpan.FromHours(1));

            var service = new RestAccessService(configuration.Object, accessClient.Object);

            Assert.That(service.Contains(_session.Token), Is.True);

            accessClient.Verify(m => m.Sessions.Get(It.IsAny<Guid>()).Result, Times.Exactly(1));
        }
    }
}