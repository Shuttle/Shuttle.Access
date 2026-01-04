using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.SqlServer;
using Shuttle.Access.Events.Role.v1;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Recall.SqlServer.Storage;
using RegisterRole = Shuttle.Access.Application.RegisterRole;

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

        var participant =
            new RegisterRoleParticipant(eventStore, idKeyRepository.Object, new Mock<IPermissionQuery>().Object);

        var addRole = new RegisterRole("role-name");

        var requestResponseMessage =
            new RequestResponseMessage<RegisterRole, RoleRegistered>(addRole);

        await participant.ProcessMessageAsync(requestResponseMessage, CancellationToken.None);

        Assert.That(requestResponseMessage.Response, Is.Not.Null);

        var @event = eventStore.FindEvent<Registered>(requestResponseMessage.Response!.Id);

        Assert.That(@event, Is.Not.Null);
        Assert.That(@event!.Name, Is.EqualTo(addRole.Name));
    }
}