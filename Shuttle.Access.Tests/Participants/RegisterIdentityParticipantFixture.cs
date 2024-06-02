using System;
using System.Threading;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Access.Tests.Participants
{
    [TestFixture]
    public class RegisterIdentityParticipantFixture
    {
        [Test]
        public void Should_be_able_to_register_an_identity()
        {
            var eventStore = new FixtureEventStore();
            var keyStore = new Mock<IKeyStore>();
            var identityQuery = new Mock<IIdentityQuery>();
            var roleQuery = new Mock<IRoleQuery>();

            var identity = new DataAccess.Query.Identity { Id = Guid.NewGuid(), Name = "name" };

            identityQuery.Setup(m => m.Count(It.IsAny<DataAccess.Query.Identity.Specification>())).Returns(1);

            keyStore.Setup(m => m.Find(Identity.Key(identity.Name), null)).Returns((Guid?)null);

            var participant =
                new RegisterIdentityParticipant(eventStore, keyStore.Object, identityQuery.Object, roleQuery.Object);

            var registerIdentity = new RegisterIdentity
            {
                Name = "name"
            };

            var requestResponseMessage =
                new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(registerIdentity);

            participant.ProcessMessage(
                new ParticipantContext<RequestResponseMessage<RegisterIdentity, IdentityRegistered>>(
                    requestResponseMessage, CancellationToken.None));

            Assert.That(requestResponseMessage.Ok, Is.True);
            Assert.That(requestResponseMessage.Response, Is.Not.Null);

            var @event = eventStore.FindEvent<Registered>(requestResponseMessage.Response.Id);

            Assert.That(@event, Is.Not.Null);

            Assert.That(requestResponseMessage.Response.Name, Is.EqualTo(identity.Name));
        }
    }
}