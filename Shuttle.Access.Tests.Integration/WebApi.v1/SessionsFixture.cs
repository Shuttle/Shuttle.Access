using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class SessionsFixture : WebApiFixture
{
    private const string Permission = "integration://system-permission";

    [Test]
    public async Task Should_be_able_to_get_a_session_using_the_token_async()
    {
        var sessionQuery = new Mock<ISessionQuery>();
        var session = new Access.DataAccess.Query.Session
        {
            Token = Guid.NewGuid()
        };

        sessionQuery.Setup(m => m.GetAsync(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult(session));

        using (var httpClient = Factory.WithWebHostBuilder(builder =>
               {
                   builder.ConfigureTestServices(services =>
                   {
                       services.AddSingleton(sessionQuery.Object);
                   });
               }).CreateDefaultClient())
        {
            var client = await GetClient(httpClient).RegisterSessionAsync();

            var response = await client.Sessions.GetAsync(session.Token);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.Content, Is.Not.Null);
            Assert.That(response.Content.Token, Is.EqualTo(session.Token));
        }
    }

    [Test]
    public async Task Should_be_able_to_get_session_permissions_async()
    {
        var sessionRepository = new Mock<ISessionRepository>();
        var session = new Session(Guid.NewGuid(), Guid.NewGuid(), "identity", DateTime.UtcNow, DateTime.UtcNow.AddSeconds(15))
            .AddPermission(Permission);

        sessionRepository.Setup(m => m.FindAsync(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult(session));

        using (var httpClient = Factory.WithWebHostBuilder(builder =>
               {
                   builder.ConfigureTestServices(services =>
                   {
                       services.AddSingleton(sessionRepository.Object);
                   });
               }).CreateDefaultClient())
        {
            var client = await GetClient(httpClient).RegisterSessionAsync();

            var response = await client.Sessions.GetPermissionsAsync(session.Token);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.IsSuccessStatusCode, Is.True);
            Assert.That(response.Content, Is.Not.Null);
            Assert.That(response.Content.Find(item => item.Equals(Permission, StringComparison.InvariantCultureIgnoreCase)), Is.Not.Null);
        }
    }
}