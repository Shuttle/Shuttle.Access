using Moq;
using NUnit.Framework;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Server.v1.MessageHandlers;
using Shuttle.Hopper;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Tests.Handlers;

[TestFixture]
public class RemoveIdentityHandlerFixture
{
    [Test]
    public async Task Should_be_able_to_remove_identity_async()
    {
        var eventStore = new FixtureEventStore();
        var removeIdentity = new RemoveIdentity
        {
            Id = Guid.NewGuid()
        };

        var handler = new RemoveIdentityHandler(new Mock<IBus>().Object, eventStore, new Mock<IIdKeyRepository>().Object);

        await handler.HandleAsync(removeIdentity, CancellationToken.None);

        Assert.That((await eventStore.GetAsync(removeIdentity.Id)).Count, Is.EqualTo(1));

        var removed = eventStore.FindEvent<Removed>(removeIdentity.Id);

        Assert.That(removed, Is.TypeOf<Removed>());
    }
}