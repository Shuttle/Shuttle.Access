using System;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Recall;
using RoleAdded = Shuttle.Access.Events.Identity.v1.RoleAdded;

namespace Shuttle.Access.Tests.Participants
{
    [TestFixture]
    public class SetIdentityRoleStatusParticipantFixture
    {
        [Test]
        public void Should_be_able_to_review_with_no_administrator_role()
        {
            var eventStore = new FixtureEventStore();
            var participant = new SetIdentityRoleStatusParticipant(eventStore);

            var identityId = Guid.NewGuid();

            var setIdentityRoleStatus = new SetIdentityRoleStatus
            {
                RoleId = Guid.NewGuid(),
                Active = true,
                IdentityId = identityId
            };
            
            participant.ProcessMessage(new ParticipantContext<SetIdentityRoleStatus>(setIdentityRoleStatus, new CancellationToken()));

            var eventStream = eventStore.Get(identityId);

            Assert.That(eventStream.Count, Is.EqualTo(1));
            Assert.That(((RoleAdded)eventStream.GetEvents(EventStream.EventRegistrationType.All).First().Event).RoleId, Is.EqualTo(setIdentityRoleStatus.RoleId));
        }
    }
}