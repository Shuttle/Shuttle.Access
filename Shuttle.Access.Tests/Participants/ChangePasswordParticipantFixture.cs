using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using PasswordSet = Shuttle.Access.Events.Identity.v1.PasswordSet;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class ChangePasswordParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_change_password_async()
    {
        var now = DateTimeOffset.UtcNow;
        var changePassword = new ChangePassword { Token = Guid.NewGuid(), NewPassword = "new-password" };
        var eventStore = new FixtureEventStore();
        var sessionRepository = new Mock<ISessionRepository>();
        var hashingService = new HashingService();
        var sessionTokenHash = hashingService.Sha256(changePassword.Token.Value.ToString("D"));
        var passwordHash = hashingService.Sha256(changePassword.NewPassword);

        var session = new Session(sessionTokenHash, Guid.NewGuid(), "identity-name", now, now.AddSeconds(5));
        
        sessionRepository.Setup(m => m.FindAsync(sessionTokenHash, CancellationToken.None)).Returns(Task.FromResult(session)!);

        var participant = new ChangePasswordParticipant(hashingService, sessionRepository.Object, eventStore);

        (await eventStore.GetAsync(session.IdentityId)).Add(new Registered
        {
            Activated = true,
            DateRegistered = DateTimeOffset.Now,
            Name = "user"
        });

        await participant.ProcessMessageAsync(new(changePassword), CancellationToken.None);

        var @event = eventStore.FindEvent<PasswordSet>(session.IdentityId);

        Assert.That(@event, Is.Not.Null);
        Assert.That(@event!.PasswordHash, Is.EqualTo(passwordHash));
    }
}