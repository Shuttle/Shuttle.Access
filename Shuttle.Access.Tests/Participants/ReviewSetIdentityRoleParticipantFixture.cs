using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class ReviewSetIdentityRoleParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_review_with_no_administrator_role_async()
    {
        var roleQuery = new Mock<IRoleQuery>();

        roleQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Query.Role.Specification>(), default)).Returns(Task.FromResult(Enumerable.Empty<Messages.v1.Role>()));

        var participant = new ReviewSetIdentityRoleParticipant(roleQuery.Object, new Mock<IIdentityQuery>().Object);
        var reviewRequest = new RequestMessage<SetIdentityRole>(new());

        await participant.ProcessMessageAsync(new ParticipantContext<RequestMessage<SetIdentityRole>>(reviewRequest, new()));

        Assert.That(reviewRequest.Ok, Is.True);
    }

    [Test]
    public async Task Should_be_able_to_fail_review_with_last_administrator_role_async()
    {
        var roleId = Guid.NewGuid();
        var roleQuery = new Mock<IRoleQuery>();

        roleQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Query.Role.Specification>(), default)).Returns(Task.FromResult(new List<Messages.v1.Role>
        {
            new()
            {
                Id = roleId,
                Name = "Administrator"
            }
        }.AsEnumerable()));

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.AdministratorCountAsync(CancellationToken.None)).Returns(ValueTask.FromResult(1));

        var participant = new ReviewSetIdentityRoleParticipant(roleQuery.Object, identityQuery.Object);
        var reviewRequest = new RequestMessage<SetIdentityRole>(new() { RoleId = roleId, IdentityId = Guid.NewGuid(), Active = false });

        await participant.ProcessMessageAsync(new ParticipantContext<RequestMessage<SetIdentityRole>>(reviewRequest, new()));

        Assert.That(reviewRequest.Ok, Is.False);
        Assert.That(reviewRequest.Message, Is.EqualTo("last-administrator"));
    }
}