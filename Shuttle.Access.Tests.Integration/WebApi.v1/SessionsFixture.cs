using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests.Integration.WebApi.v1
{
    public class SessionsFixture : WebApiFixture
    {
        const string permission = "integration://system-permission";

        [Test]
        public void Should_be_able_to_get_a_session_using_the_token()
        {
            var sessionQuery = new Mock<ISessionQuery>();
            var session = new Access.DataAccess.Query.Session
            {
                Token = Guid.NewGuid()
            };

            sessionQuery.Setup(m => m.Get(It.IsAny<Guid>())).Returns(session);

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(sessionQuery.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).Login();

                var response = client.Sessions.Get(session.Token).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                Assert.That(response.Content.Token, Is.EqualTo(session.Token));
            }
        }

        [Test]
        public void Should_be_able_to_get_session_permissions()
        {
            var sessionRepository = new Mock<ISessionRepository>();
            var session = new Session(Guid.NewGuid(), Guid.NewGuid(), "identity", DateTime.Now, DateTime.Now.AddSeconds(15))
                .AddPermission(permission);

            sessionRepository.Setup(m => m.Find(It.IsAny<Guid>())).Returns(session);

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(sessionRepository.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).Login();

                var response = client.Sessions.GetPermissions(session.Token).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                Assert.That(response.Content.Find(item=> item.Equals(permission, StringComparison.InvariantCultureIgnoreCase)), Is.Not.Null);
            }
        }
    }
}