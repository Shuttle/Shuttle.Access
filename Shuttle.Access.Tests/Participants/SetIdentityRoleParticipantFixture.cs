using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
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
        var participant = new SetIdentityRoleParticipant(eventStore);

        var identityId = Guid.NewGuid();

        var setIdentityRole = new RequestResponseMessage<SetIdentityRole, IdentityRoleSet>(new()
        {
            RoleId = Guid.NewGuid(),
            Active = true,
            IdentityId = identityId
        });

        await participant.ProcessMessageAsync(new ParticipantContext<RequestResponseMessage<SetIdentityRole, IdentityRoleSet>>(setIdentityRole, new()));

        var eventStream = eventStore.Get(identityId);

        Assert.That(eventStream.Count, Is.EqualTo(1));
        Assert.That(((RoleAdded)eventStream.GetEvents(EventStream.EventRegistrationType.All).First().Event).RoleId, Is.EqualTo(setIdentityRole.Request.RoleId));
    }
}