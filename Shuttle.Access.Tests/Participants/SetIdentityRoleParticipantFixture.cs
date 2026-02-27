using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Recall;
using RoleAdded = Shuttle.Access.Events.Identity.v1.RoleAdded;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class SetIdentityRoleParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_review_with_no_administrator_role_async()
    {
        var eventStore = new FixtureEventStore();
        var participant = new SetIdentityRoleStatusParticipant(eventStore, new Mock<IRoleQuery>().Object, new Mock<IIdentityQuery>().Object);

        var identityId = Guid.NewGuid();

        var setIdentityRole = new SetIdentityRoleStatus
        {
            AuditTenantId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            Active = true,
            IdentityId = identityId
        };

        await participant.HandleAsync(setIdentityRole);

        var eventStream = await eventStore.GetAsync(identityId);

        Assert.That(eventStream.Count, Is.EqualTo(1));
        Assert.That(((RoleAdded)eventStream.GetEvents(EventStream.EventRegistrationType.All).First().Event).RoleId, Is.EqualTo(setIdentityRole.RoleId));
    }
}