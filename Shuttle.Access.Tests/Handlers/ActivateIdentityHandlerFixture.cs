using Moq;
using NUnit.Framework;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Server.v1.MessageHandlers;

namespace Shuttle.Access.Tests.Handlers;

[TestFixture]
public class ActivateIdentityHandlerFixture
{
    [Test]
    public async Task Should_be_able_to_activate_identity_async()
    {
        var eventStore = new FixtureEventStore();
        var identityQuery = new Mock<IIdentityQuery>();

        var identity = new Query.Identity { Id = Guid.NewGuid() };

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), CancellationToken.None))
            .Returns(Task.FromResult(new List<Query.Identity> { identity }.AsEnumerable()));

        var handler = new ActivateIdentityHandler(identityQuery.Object, eventStore);

        var requestResponseMessage = new ActivateIdentity { Id = identity.Id };

        await handler.HandleAsync(requestResponseMessage, CancellationToken.None);

        var @event = eventStore.FindEvent<Activated>(identity.Id);

        Assert.That(@event, Is.Not.Null);
    }
}