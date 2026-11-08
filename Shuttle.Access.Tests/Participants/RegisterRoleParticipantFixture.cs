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

        var tenantQuery = new Mock<ITenantQuery>();

        tenantQuery.Setup(m => m.CountAsync(It.IsAny<Query.Tenant.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var participant = new RegisterRoleParticipant(eventStore, idKeyRepository.Object, new Mock<IPermissionQuery>().Object, tenantQuery.Object);

        var tenantId = Guid.NewGuid();
        var registerRole = new RegisterRole(Guid.NewGuid(), tenantId, "role-name", tenantId, "test");

        await participant.HandleAsync(registerRole, CancellationToken.None);

        var @event = eventStore.FindEvent<Registered>(registerRole.Id);

        Assert.That(@event, Is.Not.Null);

        Assert.That(registerRole.Name, Is.EqualTo(registerRole.Name));
    }

    [Test]
    public void Should_not_be_able_to_add_role_for_unknown_tenant()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();
        var tenantQuery = new Mock<ITenantQuery>();

        tenantQuery.Setup(m => m.CountAsync(It.IsAny<Query.Tenant.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var participant = new RegisterRoleParticipant(eventStore, idKeyRepository.Object, new Mock<IPermissionQuery>().Object, tenantQuery.Object);

        var tenantId = Guid.NewGuid();
        var registerRole = new RegisterRole(Guid.NewGuid(), tenantId, "role-name", tenantId, "test");

        Assert.ThrowsAsync<ApplicationException>(async () => await participant.HandleAsync(registerRole, CancellationToken.None));

        Assert.That(eventStore.FindEvent<Registered>(registerRole.Id), Is.Null);

        idKeyRepository.Verify(m => m.AddAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Should_not_be_able_to_add_role_with_unknown_permission_id()
    {
        var eventStore = new FixtureEventStore();
        var idKeyRepository = new Mock<IIdKeyRepository>();
        var tenantQuery = new Mock<ITenantQuery>();
        var permissionQuery = new Mock<IPermissionQuery>();

        tenantQuery.Setup(m => m.CountAsync(It.IsAny<Query.Tenant.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);
        permissionQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Permission.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var participant = new RegisterRoleParticipant(eventStore, idKeyRepository.Object, permissionQuery.Object, tenantQuery.Object);

        var tenantId = Guid.NewGuid();
        var registerRole = new RegisterRole(Guid.NewGuid(), tenantId, "role-name", tenantId, "test").AddPermissionId(Guid.NewGuid());

        Assert.ThrowsAsync<ApplicationException>(async () => await participant.HandleAsync(registerRole, CancellationToken.None));

        Assert.That(eventStore.FindEvent<Registered>(registerRole.Id), Is.Null);

        idKeyRepository.Verify(m => m.AddAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}