using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Data;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class ActivateIdentityParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_activate_identity_async()
    {
        var eventStore = new FixtureEventStore();
        var identityQuery = new Mock<IIdentityQuery>();

        var identity = new Data.Models.Identity { Id = Guid.NewGuid() };

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<Data.Models.Identity.Specification>(), CancellationToken.None))
            .Returns(Task.FromResult(new List<Data.Models.Identity> { identity }.AsEnumerable()));

        var participant =
            new ActivateIdentityParticipant(identityQuery.Object, eventStore);

        var requestResponseMessage =
            new RequestResponseMessage<ActivateIdentity, IdentityActivated>(new() { Id = identity.Id });

        await participant.ProcessMessageAsync(requestResponseMessage, CancellationToken.None);

        var @event = eventStore.FindEvent<Activated>(identity.Id);

        Assert.That(@event, Is.Not.Null);

        Assert.That(requestResponseMessage.Response, Is.Not.Null);
        Assert.That(requestResponseMessage.Response!.Id, Is.EqualTo(identity.Id));
    }
}