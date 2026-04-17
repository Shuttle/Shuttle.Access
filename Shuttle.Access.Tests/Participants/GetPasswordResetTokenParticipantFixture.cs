using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Identity.v1;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class GetPasswordResetTokenParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_get_password_reset_token_async()
    {
        var eventStore = new FixtureEventStore();
        var identityQuery = new Mock<IIdentityQuery>();

        var identity = new Query.Identity { Id = Guid.NewGuid() };

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), CancellationToken.None))
            .Returns(Task.FromResult(new List<Query.Identity> { identity }.AsEnumerable()));

        var participant = new GetPasswordResetTokenParticipant(identityQuery.Object, eventStore);

        var getPasswordResetToken = new GetPasswordResetToken(identity.Id);

        (await eventStore.GetAsync(identity.Id)).Add(new Activated()).Commit();

        await participant.HandleAsync(getPasswordResetToken, CancellationToken.None);
    }
}