using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.SqlServer;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class ReviewSetIdentityRoleParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_review_with_no_administrator_role_async()
    {
        var roleQuery = new Mock<IRoleQuery>();

        roleQuery.Setup(m => m.SearchAsync(It.IsAny<SqlServer.Models.Role.Specification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Enumerable.Empty<SqlServer.Models.Role>()));

        var participant = new ReviewSetIdentityRoleParticipant(roleQuery.Object, new Mock<IIdentityQuery>().Object);
        var reviewRequest = new RequestMessage<SetIdentityRoleStatus>(new());

        await participant.ProcessMessageAsync(reviewRequest);

        Assert.That(reviewRequest.Ok, Is.True);
    }

    [Test]
    public async Task Should_be_able_to_fail_review_with_last_administrator_role_async()
    {
        var roleId = Guid.NewGuid();
        var roleQuery = new Mock<IRoleQuery>();

        roleQuery.Setup(m => m.SearchAsync(It.IsAny<SqlServer.Models.Role.Specification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new List<SqlServer.Models.Role>
        {
            new()
            {
                Id = roleId,
                Name = "Access Administrator"
            }
        }.AsEnumerable()));

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.AdministratorCountAsync(CancellationToken.None)).Returns(ValueTask.FromResult(1));

        var participant = new ReviewSetIdentityRoleParticipant(roleQuery.Object, identityQuery.Object);
        var reviewRequest = new RequestMessage<SetIdentityRoleStatus>(new() { RoleId = roleId, IdentityId = Guid.NewGuid(), Active = false });

        await participant.ProcessMessageAsync(reviewRequest);

        Assert.That(reviewRequest.Ok, Is.False);
        Assert.That(reviewRequest.Message, Is.EqualTo("last-administrator"));
    }
}