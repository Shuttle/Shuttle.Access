using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using PasswordSet = Shuttle.Access.Events.Identity.v1.PasswordSet;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class ChangePasswordParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_change_password_async()
    {
        var now = DateTime.UtcNow;
        var hash = new byte[] { 0, 1, 2, 3, 4 };
        var changePassword = new ChangePassword { Token = Guid.NewGuid(), NewPassword = "new-password" };
        var session = new Session(changePassword.Token.Value, Guid.NewGuid(), "identity-name", now, now.AddSeconds(5));
        var eventStore = new FixtureEventStore();
        var sessionRepository = new Mock<ISessionRepository>();
        var hashingService = new Mock<IHashingService>();

        sessionRepository.Setup(m => m.FindAsync(session.Token, CancellationToken.None)).Returns(Task.FromResult(session));
        hashingService.Setup(m => m.Sha256(changePassword.NewPassword)).Returns(hash);

        var participant = new ChangePasswordParticipant(hashingService.Object, sessionRepository.Object, eventStore);

        (await eventStore.GetAsync(session.IdentityId)).Add(new Registered
        {
            Activated = true,
            DateRegistered = DateTime.Now,
            Name = "user"
        });

        await participant.ProcessMessageAsync(
            new ParticipantContext<RequestMessage<ChangePassword>>(
                new(changePassword), CancellationToken.None));

        var @event = eventStore.FindEvent<PasswordSet>(session.IdentityId);

        Assert.That(@event, Is.Not.Null);
        Assert.That(@event.PasswordHash, Is.EqualTo(hash));
    }
}