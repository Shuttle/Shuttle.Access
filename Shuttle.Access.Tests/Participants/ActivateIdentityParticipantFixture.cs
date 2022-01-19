using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants
{
    [TestFixture]
    public class ActivateIdentityParticipantFixture
    {
        [Test]
        public void Should_be_able_to_activate_identity()
        {
            var eventStore = new FixtureEventStore();
            var identityQuery = new Mock<IIdentityQuery>();

            var identity = new DataAccess.Query.Identity { Id = Guid.NewGuid() };

            identityQuery.Setup(m => m.Search(It.IsAny<DataAccess.Query.Identity.Specification>())).Returns(
                new List<DataAccess.Query.Identity>
                {
                    identity
                });

            var participant =
                new ActivateIdentityParticipant(identityQuery.Object, eventStore);

            var requestResponseMessage =
                new RequestResponseMessage<ActivateIdentity, IdentityActivated>(new ActivateIdentity
                    { Id = identity.Id   });

            participant.ProcessMessage(
                new ParticipantContext<RequestResponseMessage<ActivateIdentity, IdentityActivated>>(
                    requestResponseMessage,
                    CancellationToken.None));

            var @event = eventStore.FindEvent<Activated>(identity.Id);

            Assert.That(@event, Is.Not.Null);

            Assert.That(requestResponseMessage.Response, Is.Not.Null);
            Assert.That(requestResponseMessage.Response.Id, Is.EqualTo(identity.Id));
        }
    }
}