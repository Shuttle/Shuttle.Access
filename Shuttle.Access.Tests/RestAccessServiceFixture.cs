using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Refit;
using Shuttle.Access.RestClient;
using Shuttle.Access.RestClient.v1;

namespace Shuttle.Access.Tests
{
    [TestFixture]
    public class RestAccessServiceFixture
    {
        private readonly Session _session = new(Guid.NewGuid(), Guid.NewGuid(), "test-user", DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1));

        [Test]
        public void Should_be_able_check_for_non_existent_session()
        {
            var accessClient = new Mock<IAccessClient>();
            var sessionsApi = new Mock<ISessionsApi>();

            sessionsApi.Setup(m => m.Get(It.IsAny<Guid>()).Result).Returns(new ApiResponse<DataAccess.Query.Session>(new HttpResponseMessage(HttpStatusCode.BadRequest), null, new RefitSettings()));
            accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);

            var service = new RestAccessService(Options.Create(new AccessOptions()), accessClient.Object);

            Assert.That(service.Contains(Guid.NewGuid()), Is.False);
        }

        [Test]
        public void Should_be_able_check_for_and_cache_existing_session()
        {
            var accessClient = new Mock<IAccessClient>();
            var sessionsApi = new Mock<ISessionsApi>();

            sessionsApi.Setup(m => m.Get(It.IsAny<Guid>()).Result).Returns(new ApiResponse<DataAccess.Query.Session>(new HttpResponseMessage(HttpStatusCode.OK), new DataAccess.Query.Session { Permissions = new List<string>() }, new RefitSettings()));

            accessClient.Setup(m => m.Sessions).Returns(sessionsApi.Object);

            var service = new RestAccessService(Options.Create(new AccessOptions
            {
                SessionDuration = TimeSpan.FromHours(1)
            }), accessClient.Object);

            Assert.That(service.Contains(_session.Token), Is.True);

            accessClient.Verify(m => m.Sessions.Get(It.IsAny<Guid>()).Result, Times.Exactly(1));
        }
    }
}