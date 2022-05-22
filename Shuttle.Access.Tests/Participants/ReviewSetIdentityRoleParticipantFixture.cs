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
    public class ReviewSetIdentityRoleParticipantFixture
    {
        [Test]
        public void Should_be_able_to_review_with_no_administrator_role()
        {
            var roleQuery = new Mock<IRoleQuery>();

            roleQuery.Setup(m => m.Search(It.IsAny<DataAccess.Query.Role.Specification>())).Returns(Enumerable.Empty<DataAccess.Query.Role>());

            var participant = new ReviewSetIdentityRoleParticipant(roleQuery.Object, new Mock<IIdentityQuery>().Object);
            var reviewRequest = new RequestMessage<SetIdentityRole>(new SetIdentityRole());

            participant.ProcessMessage(new ParticipantContext<RequestMessage<SetIdentityRole>>(reviewRequest, new CancellationToken()));

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
                    Name = "Administrator"
                }
            });

            var identityQuery = new Mock<IIdentityQuery>();

            identityQuery.Setup(m => m.AdministratorCount()).Returns(1);

            var participant = new ReviewSetIdentityRoleParticipant(roleQuery.Object, identityQuery.Object);
            var reviewRequest = new RequestMessage<SetIdentityRole>(new SetIdentityRole { RoleId = roleId, IdentityId = Guid.NewGuid(), Active = false });

            participant.ProcessMessage(new ParticipantContext<RequestMessage<SetIdentityRole>>(reviewRequest, new CancellationToken()));

            Assert.That(reviewRequest.Ok, Is.False);
            Assert.That(reviewRequest.Message, Is.EqualTo("last-administrator"));
        }
    }
}