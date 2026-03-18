using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Identity.v1;
using PasswordSet = Shuttle.Access.Events.Identity.v1.PasswordSet;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class ChangePasswordParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_change_password_async()
    {
        var now = DateTimeOffset.UtcNow;
        var changePassword = ChangePassword.UseToken(Guid.NewGuid(), "new-password", Guid.NewGuid(), "audit-identity-name");
        var eventStore = new FixtureEventStore();
        var sessionRepository = new Mock<ISessionRepository>();
        var hashingService = new HashingService();
        var sessionTokenHash = hashingService.Sha256(changePassword.Token!.Value.ToString("D"));
        var passwordHash = hashingService.Sha256(changePassword.NewPassword);

        var session = new Session(Guid.NewGuid(), sessionTokenHash, Guid.NewGuid(), Guid.NewGuid(), now, now.AddSeconds(5));

        sessionRepository.Setup(m => m.SearchAsync(It.IsAny<Query.Session.Specification>(), CancellationToken.None)).ReturnsAsync([session]);

        var participant = new ChangePasswordParticipant(hashingService, sessionRepository.Object, eventStore);

        (await eventStore.GetAsync(session.IdentityId)).Add(new Registered
        {
            Activated = true,
            DateRegistered = DateTimeOffset.Now,
            Name = "user"
        });

        await participant.HandleAsync(changePassword, CancellationToken.None);

        var @event = eventStore.FindEvent<PasswordSet>(session.IdentityId);

        Assert.That(@event, Is.Not.Null);
        Assert.That(@event!.PasswordHash, Is.EqualTo(passwordHash));
    }
}