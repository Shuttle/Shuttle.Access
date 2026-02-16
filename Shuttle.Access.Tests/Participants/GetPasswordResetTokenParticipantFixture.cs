using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Identity.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class GetPasswordResetTokenParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_get_password_reset_token_async()
    {
        var eventStore = new FixtureEventStore();
        var identityQuery = new Mock<IIdentityQuery>();

        var identity = new SqlServer.Models.Identity { Id = Guid.NewGuid() };

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<IdentitySpecification>(), CancellationToken.None))
            .Returns(Task.FromResult(new List<SqlServer.Models.Identity> { identity }.AsEnumerable()));

        var participant = new GetPasswordResetTokenParticipant(identityQuery.Object, eventStore);

        var getPasswordResetToken = new GetPasswordResetToken { Name = "identity-name" };
        var requestResponseMessage = new RequestResponseMessage<GetPasswordResetToken, Guid>(getPasswordResetToken);

        await participant.HandleAsync(requestResponseMessage, CancellationToken.None);

        Assert.That(requestResponseMessage.Ok, Is.False);

        (await eventStore.GetAsync(identity.Id)).Add(new Activated()).Commit();

        requestResponseMessage = new(getPasswordResetToken);

        await participant.HandleAsync(requestResponseMessage, CancellationToken.None);

        Assert.That(requestResponseMessage.Ok, Is.True);
    }
}