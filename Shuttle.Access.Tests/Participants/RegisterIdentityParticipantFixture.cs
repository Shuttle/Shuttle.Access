using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Mediator;
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
        var roleQuery = new Mock<IRoleQuery>();

        var identity = new Messages.v1.Identity { Id = Guid.NewGuid(), Name = "name" };

        identityQuery.Setup(m => m.CountAsync(It.IsAny<SqlServer.Models.Identity.Specification>(), CancellationToken.None)).Returns(ValueTask.FromResult(1));

        idKeyRepository.Setup(m => m.FindAsync(Identity.Key(identity.Name), CancellationToken.None)).ReturnsAsync(await ValueTask.FromResult((Guid?)null));

        var participant = new RegisterIdentityParticipant(eventStore, idKeyRepository.Object, identityQuery.Object, roleQuery.Object);

        var registerIdentity = new RegisterIdentity
        {
            Name = "name"
        };

        var requestResponseMessage = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(registerIdentity);

        await participant.ProcessMessageAsync(requestResponseMessage, CancellationToken.None);

        Assert.That(requestResponseMessage.Ok, Is.True);
        Assert.That(requestResponseMessage.Response, Is.Not.Null);

        var @event = eventStore.FindEvent<Registered>(requestResponseMessage.Response!.Id);

        Assert.That(@event, Is.Not.Null);

        Assert.That(requestResponseMessage.Response.Name, Is.EqualTo(identity.Name));
    }
}