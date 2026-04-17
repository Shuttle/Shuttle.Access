using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Permission.v1;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class RegisterPermissionParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_register_permission_async()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();

        idKeyRepository.Setup(m => m.ContainsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(await ValueTask.FromResult(false));

        var participant = new RegisterPermissionParticipant(eventStore, idKeyRepository.Object);

        var tenantId = Guid.NewGuid();
        var registerPermission = new Application.RegisterPermission(Guid.NewGuid(),"permission-name", "permission-description",PermissionStatus.Active,tenantId,"test");

        await participant.HandleAsync(registerPermission, CancellationToken.None);

        var @event = eventStore.FindEvent<Registered>(registerPermission.Id);

        Assert.That(@event, Is.Not.Null);

        Assert.That(registerPermission.Name, Is.EqualTo(registerPermission.Name));
    }
}