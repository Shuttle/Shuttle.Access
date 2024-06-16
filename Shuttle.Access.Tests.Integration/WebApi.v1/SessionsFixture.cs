using System;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class SessionsFixture : WebApiFixture
{
    private const string Permission = "integration://system-permission";

    [Test]
    public async Task Should_be_able_to_register_a_session_async()
    {
        var sessionService = new Mock<ISessionService>();

        var factory = new FixtureWebApplicationFactory(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(sessionService.Object);
            });
        });

        var session = new Session(Guid.NewGuid(), Guid.NewGuid(), "identity-name", DateTime.Now, DateTime.Now);

        factory.SessionService.Setup(m => m.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult(RegisterSessionResult.Success(session)));

        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/v1/sessions/",
            new RegisterSession
            {
                IdentityName = "identity",
                Password = "password"
            });

        var sessionRegistered = response.Content.ReadFromJsonAsync<SessionRegistered>();

        Assert.That(sessionRegistered, Is.Not.Null);
        Assert.That(sessionRegistered.IsCompletedSuccessfully, Is.True);
        Assert.That(sessionRegistered.Result?.Token, Is.EqualTo(session.Token));
        Assert.That(sessionRegistered.Result?.IdentityName, Is.EqualTo(session.IdentityName));

        factory.DatabaseContextFactory.Verify(m => m.Create(), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_get_a_session_using_the_token_async()
    {
        var sessionQuery = new Mock<ISessionQuery>();
        var session = new Access.DataAccess.Query.Session
        {
            Token = Guid.NewGuid()
        };

        sessionQuery.Setup(m => m.GetAsync(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult(session));

        var factory = new FixtureWebApplicationFactory(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton(sessionQuery.Object);
            });
        });

        var client = factory.CreateClient();

        await client.PostAsJsonAsync(
            "/v1/sessions",
            new RegisterSession
            {
                IdentityName = "identity",
                Password = "password"
            });

        var response = await client.GetFromJsonAsync<Access.DataAccess.Query.Session>($"/v1/sessions/{session.Token}");

        Assert.That(response, Is.Not.Null);
        Assert.That(response.Token, Is.EqualTo(session.Token));
    }

    //[Test]
    //public async Task Should_be_able_to_get_session_permissions_async()
    //{
    //    var sessionRepository = new Mock<ISessionRepository>();
    //    var session = new Session(Guid.NewGuid(), Guid.NewGuid(), "identity", DateTime.UtcNow, DateTime.UtcNow.AddSeconds(15))
    //        .AddPermission(Permission);

    //    sessionRepository.Setup(m => m.FindAsync(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult(session));

    //    using (var httpClient = Factory.WithWebHostBuilder(builder =>
    //           {
    //               builder.ConfigureTestServices(services =>
    //               {
    //                   services.AddSingleton(sessionRepository.Object);
    //               });
    //           }).CreateDefaultClient())
    //    {
    //        var client = await GetClient(httpClient).RegisterSessionAsync();

    //        var response = await client.Sessions.GetPermissionsAsync(session.Token);

    //        Assert.That(response, Is.Not.Null);
    //        Assert.That(response.IsSuccessStatusCode, Is.True);
    //        Assert.That(response.Content, Is.Not.Null);
    //        Assert.That(response.Content.Find(item => item.Equals(Permission, StringComparison.InvariantCultureIgnoreCase)), Is.Not.Null);
    //    }
    //}
}