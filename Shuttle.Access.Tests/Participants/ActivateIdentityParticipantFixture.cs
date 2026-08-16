using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Identity.v1;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class ActivateIdentityParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_activate_identity_async()
    {
        var eventStore = new FixtureEventStore();
        var identityQuery = new Mock<IIdentityQuery>();

        var identity = new Query.Identity { Id = Guid.NewGuid() };

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), CancellationToken.None))
            .Returns(Task.FromResult(new List<Query.Identity> { identity }.AsEnumerable()));

        var participant = new ActivateIdentityParticipant(identityQuery.Object, eventStore);

        var message = new ActivateIdentity(identity.Id, string.Empty, Guid.NewGuid(), "system");

        await participant.HandleAsync(message, CancellationToken.None);

        var @event = eventStore.FindEvent<Activated>(identity.Id);

        Assert.That(@event, Is.Not.Null);
    }
}
