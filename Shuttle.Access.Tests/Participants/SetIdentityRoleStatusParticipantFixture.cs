using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Mediator;
using Shuttle.Recall;
using RoleAdded = Shuttle.Access.Events.Identity.v1.RoleAdded;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class SetIdentityRoleStatusParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_activate_role_async()
    {
        var identityId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var eventStore = new FixtureEventStore();
        var roleQuery = new Mock<IRoleQuery>();

        roleQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Role.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = roleId,
                TenantId = tenantId
            }
        ]);

        var eventStream = await eventStore.GetAsync(identityId);

        eventStream.Add(new Events.Identity.v1.Registered
        {
            Name = "identity"
        });

        eventStream.Add(new Events.Identity.v1.TenantAdded
        {
            TenantId = tenantId
        });

        eventStream.Commit();

        var participant = new SetIdentityRoleStatusParticipant(eventStore, roleQuery.Object, new Mock<IMediator>().Object);

        var setIdentityRole = new SetIdentityRoleStatus(identityId, roleId, true, Guid.NewGuid(), "system");

        await participant.HandleAsync(setIdentityRole);

        Assert.That(eventStream.Count, Is.EqualTo(3));
        Assert.That(((RoleAdded)eventStream.GetEvents(EventStream.EventRegistrationType.All).Last().Event).RoleId, Is.EqualTo(setIdentityRole.RoleId));
    }
}
