using System;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task Should_be_able_to_register_an_identity_async()
        {
            var eventStore = new FixtureEventStore();
            var keyStore = new Mock<IKeyStore>();
            var identityQuery = new Mock<IIdentityQuery>();
            var roleQuery = new Mock<IRoleQuery>();

            var identity = new Messages.v1.Identity { Id = Guid.NewGuid(), Name = "name" };

            identityQuery.Setup(m => m.CountAsync(It.IsAny<DataAccess.Query.Identity.Specification>(), CancellationToken.None)).Returns(ValueTask.FromResult(1));

            keyStore.Setup(m => m.FindAsync(Identity.Key(identity.Name), null, CancellationToken.None)).Returns(ValueTask.FromResult((Guid?)null));

            var participant = new RegisterIdentityParticipant(eventStore, keyStore.Object, identityQuery.Object, roleQuery.Object);

            var registerIdentity = new RegisterIdentity
            {
                Name = "name"
            };

            var requestResponseMessage = new RequestResponseMessage<RegisterIdentity, IdentityRegistered>(registerIdentity);

            await participant.ProcessMessageAsync(new ParticipantContext<RequestResponseMessage<RegisterIdentity, IdentityRegistered>>(requestResponseMessage, CancellationToken.None));

            Assert.That(requestResponseMessage.Ok, Is.True);
            Assert.That(requestResponseMessage.Response, Is.Not.Null);

            var @event = eventStore.FindEvent<Registered>(requestResponseMessage.Response.Id);

            Assert.That(@event, Is.Not.Null);

            Assert.That(requestResponseMessage.Response.Name, Is.EqualTo(identity.Name));
        }
    }
}