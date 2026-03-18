using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class RegisterIdentityParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_register_an_identity_async()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();
        var identityQuery = new Mock<IIdentityQuery>();

        var identity = new WebApi.Contracts.v1.Identity { Id = Guid.NewGuid(), Name = "name" };

        identityQuery.Setup(m => m.CountAsync(It.IsAny<Query.Identity.Specification>(), CancellationToken.None)).Returns(ValueTask.FromResult(1));

        idKeyRepository.Setup(m => m.FindAsync(Identity.Key(identity.Name), CancellationToken.None)).ReturnsAsync(await ValueTask.FromResult((Guid?)null));

        var participant = new RegisterIdentityParticipant(eventStore, idKeyRepository.Object, new Mock<ITenantQuery>().Object);

        var registerIdentity = new RegisterIdentity(Guid.NewGuid(), "name", "description", string.Empty, [], "registered-by", true, Guid.NewGuid(), "audit-identity-name");

        await participant.HandleAsync(registerIdentity, CancellationToken.None);

        var @event = eventStore.FindEvent<Registered>(registerIdentity.Id);

        Assert.That(@event, Is.Not.Null);

        Assert.That(registerIdentity.Name, Is.EqualTo(identity.Name));
    }
}