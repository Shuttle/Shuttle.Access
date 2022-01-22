using System;
using System.Collections.Generic;
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
    public class IdentityRegistrationRequestedParticipantFixture
    {
        [Test]
        public void Should_be_able_to_request_identity_registration()
        {
            var authorizationService = new Mock<IAuthorizationService>();
            var anonymousPermissions = authorizationService.As<IAnonymousPermissions>();
            var identityQuery = new Mock<IIdentityQuery>();

            identityQuery.Setup(m => m.Search(It.IsAny<DataAccess.Query.Identity.Specification>())).Returns(new List<DataAccess.Query.Identity>());

            anonymousPermissions.Setup(m => m.AnonymousPermissions()).Returns(new List<string> { Permissions.Register.Identity });

            var participant = new IdentityRegistrationRequestedParticipant(authorizationService.Object ,identityQuery.Object);

            var identityRegistrationRequested = new IdentityRegistrationRequested(null);

            participant.ProcessMessage(new ParticipantContext<IdentityRegistrationRequested>(identityRegistrationRequested, CancellationToken.None));

            Assert.That(identityRegistrationRequested.IsAllowed, Is.True);
            Assert.That(identityRegistrationRequested.IsActivationAllowed, Is.True);
        }
    }
}