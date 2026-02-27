using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Query;
using Shuttle.Access.SqlServer;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class ReviewSetIdentityRoleParticipantFixture
{
    [Test]
    public async Task Should_be_able_to_review_with_no_administrator_role_async()
    {
        var roleQuery = new Mock<IRoleQuery>();

        roleQuery.Setup(m => m.SearchAsync(It.IsAny<RoleSpecification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(Enumerable.Empty<SqlServer.Models.Role>()));

        var participant = new ReviewIdentityRoleRemovalParticipant(roleQuery.Object, new Mock<IIdentityQuery>().Object);
        var reviewRequest = new ReviewIdentityRoleRemoval(Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThatAsync(async () => await participant.HandleAsync(reviewRequest), Throws.Nothing);
    }

    [Test]
    public async Task Should_be_able_to_fail_review_with_last_administrator_role_async()
    {
        var roleId = Guid.NewGuid();
        var roleQuery = new Mock<IRoleQuery>();

        roleQuery.Setup(m => m.SearchAsync(It.IsAny<RoleSpecification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new List<SqlServer.Models.Role>
        {
            new()
            {
                Id = roleId,
                Name = "Access Administrator"
            }
        }.AsEnumerable()));

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.AdministratorCountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).Returns(ValueTask.FromResult(1));

        var participant = new ReviewIdentityRoleRemovalParticipant(roleQuery.Object, identityQuery.Object);
        var reviewRequest = new ReviewIdentityRoleRemoval(Guid.NewGuid(), roleId);

        await participant.HandleAsync(reviewRequest);

        Assert.That(reviewRequest.IsLastAdministrator, Is.True);
    }
}