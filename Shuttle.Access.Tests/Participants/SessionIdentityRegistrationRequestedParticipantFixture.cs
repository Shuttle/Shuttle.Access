using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class RequestIdentityRegistrationParticipantFixture
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

        var participant = new RequestIdentityRegistrationParticipant(new Mock<IServiceBus>().Object, sessionRepository.Object, new Mock<IMediator>().Object);

        var identityRegistrationRequested = new RequestIdentityRegistration(new() { Name = "identity" }).WithSessionToken(session.Token);

        await participant.ProcessMessageAsync(new ParticipantContext<RequestIdentityRegistration>(identityRegistrationRequested, CancellationToken.None));

        Assert.That(identityRegistrationRequested.IsAllowed, Is.True);
        Assert.That(identityRegistrationRequested.IsActivationAllowed, Is.True);
    }
}