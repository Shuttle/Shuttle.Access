using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Events.Role.v1;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class SetRolePermissionStatusParticipantFixture
{
    private static Mock<IPermissionQuery> CreatePermissionQuery(params Guid[] knownPermissionIds)
    {
        var permissionQuery = new Mock<IPermissionQuery>();

        permissionQuery
            .Setup(m => m.SearchAsync(It.IsAny<Query.Permission.Specification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Query.Permission.Specification specification, CancellationToken _) =>
                knownPermissionIds.Where(id => specification.Ids.Contains(id)).Select(id => new Query.Permission { Id = id }));

        return permissionQuery;
    }

    [Test]
    public async Task Should_be_able_to_add_known_permission_to_role_async()
    {
        var eventStore = new FixtureEventStore();
        var permissionId = Guid.NewGuid();
        var participant = new SetRolePermissionStatusParticipant(eventStore, CreatePermissionQuery(permissionId).Object);

        var message = new SetRolePermissionStatus(Guid.NewGuid(), permissionId, true, Guid.NewGuid(), "test");

        await participant.HandleAsync(message, CancellationToken.None);

        Assert.That(eventStore.FindEvent<PermissionAdded>(message.RoleId), Is.Not.Null);
    }

    [Test]
    public void Should_not_be_able_to_add_unknown_permission_to_role()
    {
        var eventStore = new FixtureEventStore();
        var participant = new SetRolePermissionStatusParticipant(eventStore, CreatePermissionQuery().Object);

        var message = new SetRolePermissionStatus(Guid.NewGuid(), Guid.NewGuid(), true, Guid.NewGuid(), "test");

        Assert.ThrowsAsync<ApplicationException>(async () => await participant.HandleAsync(message, CancellationToken.None));

        Assert.That(eventStore.FindEvent<PermissionAdded>(message.RoleId), Is.Null);
    }
}
