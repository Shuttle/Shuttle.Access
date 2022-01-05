using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants
{
    [TestFixture]
    public class ReviewSetIdentityRoleStatusParticipantFixture
    {
        [Test]
        public void Should_be_able_to_review_with_no_administrator_role()
        {
            var roleQuery = new Mock<IRoleQuery>();

            roleQuery.Setup(m => m.Search(It.IsAny<DataAccess.Query.Role.Specification>())).Returns(Enumerable.Empty<DataAccess.Query.Role>());

            var participant = new ReviewSetIdentityRoleStatusParticipant(roleQuery.Object, new Mock<IIdentityQuery>().Object);
            var reviewRequest = new ReviewRequest<SetIdentityRoleStatus>(new SetIdentityRoleStatus());

            participant.ProcessMessage(new ParticipantContext<ReviewRequest<SetIdentityRoleStatus>>(reviewRequest, new CancellationToken()));

            Assert.That(reviewRequest.Ok, Is.True);
        }

        [Test]
        public void Should_be_able_to_fail_review_with_last_administrator_role()
        {
            var roleId = Guid.NewGuid();
            var roleQuery = new Mock<IRoleQuery>();

            roleQuery.Setup(m => m.Search(It.IsAny<DataAccess.Query.Role.Specification>())).Returns(new List<DataAccess.Query.Role>
            {
                new()
                {
                    Id = roleId,
                    RoleName = "Administrator"
                }
            });

            var identityQuery = new Mock<IIdentityQuery>();

            identityQuery.Setup(m => m.AdministratorCount()).Returns(1);

            var participant = new ReviewSetIdentityRoleStatusParticipant(roleQuery.Object, identityQuery.Object);
            var reviewRequest = new ReviewRequest<SetIdentityRoleStatus>(new SetIdentityRoleStatus { RoleId = roleId, IdentityId = Guid.NewGuid(), Active = false });

            participant.ProcessMessage(new ParticipantContext<ReviewRequest<SetIdentityRoleStatus>>(reviewRequest, new CancellationToken()));

            Assert.That(reviewRequest.Ok, Is.False);
            Assert.That(reviewRequest.Message, Is.EqualTo("last-administrator"));
        }
    }
}