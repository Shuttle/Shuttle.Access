using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Shuttle.Access.Application;
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

            var eventStream = eventStore.Get(removeIdentity.Id);

            Assert.That(eventStream.Count, Is.EqualTo(1));

            var removed = eventStream.GetEvents().First().Event;

            Assert.That(removed, Is.TypeOf<Removed>());
            Assert.That(((Removed)removed).Id, Is.EqualTo(removeIdentity.Id));
        }
    }
}