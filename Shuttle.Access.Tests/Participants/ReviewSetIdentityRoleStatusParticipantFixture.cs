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
            var participant = new ReviewSetIdentityRoleStatusParticipant(new Mock<IRoleQuery>().Object, new Mock<IIdentityQuery>().Object);
            var reviewRequest = new ReviewRequest<SetIdentityRoleStatus>(new SetIdentityRoleStatus());

            participant.ProcessMessage(new ParticipantContext<ReviewRequest<SetIdentityRoleStatus>>(reviewRequest, new CancellationToken()));

            Assert.That(reviewRequest.Ok, Is.True);
        }
    }
}