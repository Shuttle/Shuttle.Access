﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class RequestIdentityRegistrationParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_request_identity_registration_using_a_session_async()
    {
        var now = DateTimeOffset.UtcNow;
        var identityId = Guid.NewGuid();
        var session = new Session(Guid.NewGuid().ToByteArray(), identityId, "identity-name", now, now.AddSeconds(5))
            .AddPermission(AccessPermissions.Identities.Register)
            .AddPermission(AccessPermissions.Identities.Activate);
        var sessionRepository = new Mock<ISessionRepository>();

        sessionRepository.Setup(m => m.FindAsync(session.IdentityId, CancellationToken.None)).Returns(Task.FromResult(session)!);

        var serviceBus = new Mock<IServiceBus>();
        var participant = new RequestIdentityRegistrationParticipant(serviceBus.Object, new HashingService(), sessionRepository.Object, new Mock<IMediator>().Object);

        var identityRegistrationRequested = new RequestIdentityRegistration(new() { Name = "identity" }).WithIdentityId(identityId);

        await participant.ProcessMessageAsync(new ParticipantContext<RequestIdentityRegistration>(identityRegistrationRequested, CancellationToken.None));

        Assert.That(identityRegistrationRequested.IsAllowed, Is.True);
        Assert.That(identityRegistrationRequested.IsActivationAllowed, Is.True);

        serviceBus.Verify(m => m.SendAsync(It.IsAny<RegisterIdentity>(), null), Times.Once);
    }
}