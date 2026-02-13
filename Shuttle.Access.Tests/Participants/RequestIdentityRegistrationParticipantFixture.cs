using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Core.Mediator;
using Shuttle.Hopper;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class RequestIdentityRegistrationParticipantFixture
{
    private readonly Guid _tenantId = Guid.NewGuid();

    [Test]
    public async Task Should_be_able_to_request_identity_registration_using_a_session_async()
    {
        var now = DateTimeOffset.UtcNow;
        var identityId = Guid.NewGuid();
        var session = new Session( Guid.NewGuid(), Guid.NewGuid().ToByteArray(), identityId, "identity-name", now, now.AddSeconds(5))
            .WithTenantId(_tenantId)
            .AddPermission(new(Guid.NewGuid(), AccessPermissions.Identities.Register))
            .AddPermission(new(Guid.NewGuid(), AccessPermissions.Identities.Activate));
        var sessionRepository = new Mock<ISessionRepository>();

        sessionRepository.Setup(m => m.SearchAsync(It.IsAny<SessionSpecification>(), CancellationToken.None)).ReturnsAsync([session]);

        var bus = new Mock<IBus>();
        var participant = new RequestIdentityRegistrationParticipant(bus.Object, sessionRepository.Object, new Mock<IMediator>().Object);

        var identityRegistrationRequested = new RequestIdentityRegistration(new() { Name = "identity" }).Authorized(_tenantId, identityId);

        await participant.ProcessMessageAsync(identityRegistrationRequested, CancellationToken.None);

        Assert.That(identityRegistrationRequested.IsAllowed, Is.True);
        Assert.That(identityRegistrationRequested.IsActivationAllowed, Is.True);

        bus.Verify(m => m.SendAsync(It.IsAny<RegisterIdentity>(), null), Times.Once);
    }
}