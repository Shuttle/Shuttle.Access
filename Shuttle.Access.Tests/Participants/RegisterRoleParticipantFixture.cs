using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Role.v2;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class RegisterRoleParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_add_role_async()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();

        idKeyRepository.Setup(m => m.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(await ValueTask.FromResult(false));

        var participant = new RegisterRoleParticipant(eventStore, idKeyRepository.Object);

        var tenantId = Guid.NewGuid();
        var registerRole = new RegisterRole(Guid.NewGuid(), tenantId, "role-name", tenantId, "test");

        await participant.HandleAsync(registerRole, CancellationToken.None);

        var @event = eventStore.FindEvent<Registered>(registerRole.Id);

        Assert.That(@event, Is.Not.Null);

        Assert.That(registerRole.Name, Is.EqualTo(registerRole.Name));
    }
}