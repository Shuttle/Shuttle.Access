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
    public class GetPasswordResetTokenParticipantFixture
    {
        [Test]
        public void Should_be_able_to_get_password_reset_token()
        {
            var eventStore = new FixtureEventStore();
            var identityQuery = new Mock<IIdentityQuery>();

            var identity = new DataAccess.Query.Identity { Id = Guid.NewGuid() };

            identityQuery.Setup(m => m.Search(It.IsAny<DataAccess.Query.Identity.Specification>())).Returns(
                new List<DataAccess.Query.Identity>
                {
                    identity
                });

            var participant = new GetPasswordResetTokenParticipant(identityQuery.Object, eventStore);

            var getPasswordResetToken = new GetPasswordResetToken { Name = "identity-name" };
            var requestResponseMessage = new RequestResponseMessage<GetPasswordResetToken, Guid>(getPasswordResetToken);

            participant.ProcessMessage(new ParticipantContext<RequestResponseMessage<GetPasswordResetToken, Guid>>(requestResponseMessage, CancellationToken.None));

            Assert.That(requestResponseMessage.Ok, Is.False);

            eventStore.Get(identity.Id).AddEvent(new Activated()).Commit();

            requestResponseMessage = new RequestResponseMessage<GetPasswordResetToken, Guid>(getPasswordResetToken);

            participant.ProcessMessage(new ParticipantContext<RequestResponseMessage<GetPasswordResetToken, Guid>>(requestResponseMessage, CancellationToken.None));

            Assert.That(requestResponseMessage.Ok, Is.True);
        }
    }
}