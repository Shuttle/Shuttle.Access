using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Tests.Participants
{
    [TestFixture]
    public class RemoveIdentityParticipantFixture
    {
        [Test]
        public async Task Should_be_able_to_remove_identity_async()
        {
            var eventStore = new FixtureEventStore();
            var removeIdentity = new RemoveIdentity
            {
                Id = Guid.NewGuid()
            };

            var participant = new RemoveIdentityParticipant(eventStore, new Mock<IKeyStore>().Object);

            await participant.ProcessMessageAsync(new ParticipantContext<RemoveIdentity>(removeIdentity, CancellationToken.None));

            Assert.That((await eventStore.GetAsync(removeIdentity.Id)).Count, Is.EqualTo(1));

            var removed = eventStore.FindEvent<Removed>(removeIdentity.Id);

            Assert.That(removed, Is.TypeOf<Removed>());
        }
    }
}