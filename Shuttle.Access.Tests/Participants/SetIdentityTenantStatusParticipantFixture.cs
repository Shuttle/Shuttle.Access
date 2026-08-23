using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Recall;
using TenantAdded = Shuttle.Access.Events.Identity.v1.TenantAdded;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class SetIdentityTenantStatusParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_add_identity_to_tenant_when_under_maximum_async()
    {
        var identityId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var eventStore = new FixtureEventStore();
        var tenantQuery = new Mock<ITenantQuery>();
        var identityQuery = new Mock<IIdentityQuery>();

        tenantQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Tenant.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = tenantId,
                Name = "tenant",
                MaximumIdentities = 2
            }
        ]);

        identityQuery.Setup(m => m.CountAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var eventStream = await eventStore.GetAsync(identityId);

        eventStream.Add(new Events.Identity.v1.Registered
        {
            Name = "identity"
        });

        eventStream.Commit();

        var participant = new SetIdentityTenantStatusParticipant(eventStore, tenantQuery.Object, identityQuery.Object);

        var setIdentityTenantStatus = new SetIdentityTenantStatus(identityId, tenantId, true, Guid.NewGuid(), "system");

        await participant.HandleAsync(setIdentityTenantStatus);

        Assert.That(eventStream.Count, Is.EqualTo(2));
        Assert.That(((TenantAdded)eventStream.GetEvents(EventStream.EventRegistrationType.All).Last().Event).TenantId, Is.EqualTo(tenantId));
    }

    [Test]
    public void Should_not_be_able_to_add_identity_to_tenant_when_at_maximum()
    {
        var identityId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var eventStore = new FixtureEventStore();
        var tenantQuery = new Mock<ITenantQuery>();
        var identityQuery = new Mock<IIdentityQuery>();

        tenantQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Tenant.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = tenantId,
                Name = "tenant",
                MaximumIdentities = 2
            }
        ]);

        identityQuery.Setup(m => m.CountAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var participant = new SetIdentityTenantStatusParticipant(eventStore, tenantQuery.Object, identityQuery.Object);

        var setIdentityTenantStatus = new SetIdentityTenantStatus(identityId, tenantId, true, Guid.NewGuid(), "system");

        Assert.ThrowsAsync<ApplicationException>(async () => await participant.HandleAsync(setIdentityTenantStatus));
    }

    [Test]
    public async Task Should_be_able_to_add_identity_to_tenant_with_unlimited_maximum_async()
    {
        var identityId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var eventStore = new FixtureEventStore();
        var tenantQuery = new Mock<ITenantQuery>();
        var identityQuery = new Mock<IIdentityQuery>();

        tenantQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Tenant.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = tenantId,
                Name = "tenant",
                MaximumIdentities = 0
            }
        ]);

        var eventStream = await eventStore.GetAsync(identityId);

        eventStream.Add(new Events.Identity.v1.Registered
        {
            Name = "identity"
        });

        eventStream.Commit();

        var participant = new SetIdentityTenantStatusParticipant(eventStore, tenantQuery.Object, identityQuery.Object);

        var setIdentityTenantStatus = new SetIdentityTenantStatus(identityId, tenantId, true, Guid.NewGuid(), "system");

        await participant.HandleAsync(setIdentityTenantStatus);

        Assert.That(eventStream.Count, Is.EqualTo(2));

        identityQuery.Verify(m => m.CountAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
