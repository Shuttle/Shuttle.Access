using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class SessionsFixture
{
    private const string Permission = "integration://system-permission";

    [Test]
    public async Task Should_be_able_to_register_a_session_async()
    {
        var factory = new FixtureWebApplicationFactory();

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

        factory.DatabaseContextFactory.Verify(m => m.Create(), Times.AtLeast(1));
        factory.SessionService.Verify(m => m.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_get_a_session_using_the_token_async()
    {
        var factory = new FixtureWebApplicationFactory();

        var session = new Access.DataAccess.Query.Session
        {
            Token = Guid.NewGuid()
        };

        factory.SessionQuery.Setup(m => m.GetAsync(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult(session));

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

    [Test]
    public async Task Should_be_able_to_get_session_permissions_async()
    {
        var factory = new FixtureWebApplicationFactory();

        var session = new Session(Guid.NewGuid(), Guid.NewGuid(), "identity", DateTime.UtcNow, DateTime.UtcNow.AddSeconds(15))
            .AddPermission(Permission);

        factory.SessionRepository.Setup(m => m.FindAsync(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult(session));

        var client = factory.CreateClient();

        var response = await client.GetFromJsonAsync<IEnumerable<string>>($"/v1/sessions/{session.Token}/permissions");

        Assert.That(response, Is.Not.Null);
        Assert.That(response.FirstOrDefault(item => item.Equals(Permission, StringComparison.InvariantCultureIgnoreCase)), Is.Not.Null);
    }
}