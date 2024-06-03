using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants
{
    [TestFixture]
    public class SessionIdentityRegistrationRequestedParticipantFixture
    {
        [Test]
        public async Task Should_be_able_to_request_identity_registration_using_a_session_async()
        {
            var now = DateTime.UtcNow;
            var session = new Session(Guid.NewGuid(), Guid.NewGuid(), "identity-name", now, now.AddSeconds(5))
                .AddPermission(Permissions.Register.Identity)
                .AddPermission(Permissions.Activate.Identity);
            var sessionRepository = new Mock<ISessionRepository>();

            sessionRepository.Setup(m => m.FindAsync(session.Token, CancellationToken.None)).Returns(Task.FromResult(session));

            var participant = new SessionIdentityRegistrationRequestedParticipant(sessionRepository.Object);

            var identityRegistrationRequested = new IdentityRegistrationRequested(session.Token);

            await participant.ProcessMessageAsync(new ParticipantContext<IdentityRegistrationRequested>(identityRegistrationRequested, CancellationToken.None));

            Assert.That(identityRegistrationRequested.IsAllowed, Is.True);
            Assert.That(identityRegistrationRequested.IsActivationAllowed, Is.True);
        }
    }
}