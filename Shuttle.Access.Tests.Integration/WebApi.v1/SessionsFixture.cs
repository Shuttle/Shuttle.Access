using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class SessionsFixture
{
    private const string Permission = "integration://system-permission";

    [Test]
    public async Task Should_be_able_to_get_a_session_using_the_token_async()
    {
        var factory = new FixtureWebApplicationFactory();

        var session = new Query.Session
        {
            Id = Guid.NewGuid(),
            IdentityId = Guid.NewGuid(),
            IdentityName = "identity",
            IdentityDescription = "identity-description",
            Permissions =
            [
                new()
                {
                    Name = "permission"
                }
            ]
        };

        factory.SessionQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Session.Specification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new List<Query.Session> { session }.AsEnumerable()));

        var client = factory.GetAccessClient();

        var response = await client.Sessions.PostSearchAsync(new());

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);
        Assert.That(response.Content!.First().IdentityId, Is.EqualTo(session.IdentityId));
    }

    [Test]
    public async Task Should_be_able_to_register_a_session_async()
    {
        var identityId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var factory = new FixtureWebApplicationFactory();

        var sessionToken = Guid.NewGuid();
        var session = new Session(Guid.NewGuid(), sessionToken.ToByteArray(), identityId, "identity-name", DateTimeOffset.Now, DateTimeOffset.Now)
            .WithTenantId(Guid.NewGuid());

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RegisterSession>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((message, _) =>
            {
                var registerSession = (RegisterSession)message;

                registerSession.WithIdentity(new()
                {
                    Id = identityId,
                    Name = "identity-name",
                    Tenants =
                    [
                        new()
                        {
                            Id = tenantId,
                            Name = "System",
                            Status = TenantStatus.Active
                        }
                    ]
                });

                registerSession.Registered(sessionToken, session);
            });

        var client = factory.GetAccessClient();

        var sessionResponse = await client.Sessions.PostAsync(
            new Access.WebApi.Contracts.v1.RegisterSession
            {
                IdentityName = "identity-name",
                Password = "password"
            });

        Assert.That(sessionResponse, Is.Not.Null);
        Assert.That(sessionResponse.IsSuccessStatusCode, Is.True);
        Assert.That(sessionResponse.Content, Is.Not.Null);
        Assert.That(sessionResponse.Content!.Token, Is.EqualTo(sessionToken));
        Assert.That(sessionResponse.Content.Session.IdentityName, Is.EqualTo(session.IdentityName));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<RegisterSession>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}