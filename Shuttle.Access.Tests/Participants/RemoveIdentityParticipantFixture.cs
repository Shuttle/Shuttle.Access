using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants
{
    [TestFixture]
    public class RemoveIdentityParticipantFixture
    {
        [Test]
        public void Should_be_able_to_remove_identity()
        {
            var eventStore = new FixtureEventStore();
            var removeIdentity = new RemoveIdentity
            {
                Id = Guid.NewGuid()
            };

            var participant = new RemoveIdentityParticipant(eventStore);

            participant.ProcessMessage(new ParticipantContext<RemoveIdentity>(removeIdentity, CancellationToken.None));

            Assert.That(eventStore.Get(removeIdentity.Id).Count, Is.EqualTo(1));

            var removed = eventStore.FindEvent<Removed>(removeIdentity.Id);

            Assert.That(removed, Is.TypeOf<Removed>());
        }
    }
}