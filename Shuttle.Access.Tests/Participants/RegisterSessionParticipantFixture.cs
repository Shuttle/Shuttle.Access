using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class RegisterSessionParticipantFixture
{
    private const string IdentityName = "some-identity";

    [Test]
    public void Should_not_be_able_to_register_a_session_when_no_registration_type_has_been_set()
    {
        var message = new RegisterSession(IdentityName);
        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, new Mock<ISessionRepository>().Object, new Mock<IIdentityQuery>().Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Exception);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_a_password()
    {
        var message = new RegisterSession(IdentityName).UsePassword("some_password");
        var authenticationService = new Mock<IAuthenticationService>();

        authenticationService.Setup(m => m.AuthenticateAsync(IdentityName, "some_password", CancellationToken.None)).ReturnsAsync(new AuthenticationResult(true, IdentityName));

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Query.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), authenticationService.Object, new Mock<IAuthorizationService>().Object, new Mock<ISessionRepository>().Object, identityQuery.Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_delegation()
    {
        var token = Guid.NewGuid();
        var message = new RegisterSession(IdentityName).UseDelegation(token);
        var sessionRepository = new Mock<ISessionRepository>();

        var now = DateTime.UtcNow;
        var session = new Session(token, Guid.NewGuid(), IdentityName, now, now.AddMinutes(1))
            .AddPermission(AccessPermissions.Sessions.Register);

        sessionRepository.Setup(m => m.FindAsync(token, CancellationToken.None)).Returns(Task.FromResult(session));

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Query.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, sessionRepository.Object, identityQuery.Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_a_token()
    {
        var token = Guid.NewGuid();
        var message = new RegisterSession(IdentityName).UseToken(token);

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Query.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var sessionRepository = new Mock<ISessionRepository>();

        sessionRepository.Setup(m => m.FindAsync(token, CancellationToken.None)).Returns(Task.FromResult(new Session(token, Guid.NewGuid(), IdentityName, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1))));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, sessionRepository.Object, identityQuery.Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_direct_registration()
    {
        var message = new RegisterSession(IdentityName).UseDirect();

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Query.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, new Mock<ISessionRepository>().Object, identityQuery.Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_new_session()
    {
        var message = new RegisterSession(IdentityName).UseDirect();

        var identityQuery = new Mock<IIdentityQuery>();
        var sessionRepository = new Mock<ISessionRepository>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Query.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, sessionRepository.Object, identityQuery.Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);

        sessionRepository.Verify(m => m.SaveAsync(It.IsAny<Session>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public void Should_be_able_to_renew_a_session()
    {
        var message = new RegisterSession(IdentityName).UseDirect();

        var identityQuery = new Mock<IIdentityQuery>();
        var sessionRepository = new Mock<ISessionRepository>();

        var now = DateTime.UtcNow;
        var session = new Session(Guid.NewGuid(), Guid.NewGuid(), IdentityName, now, now.AddMinutes(-5));

        sessionRepository.Setup(m => m.FindAsync(IdentityName, CancellationToken.None)).Returns(Task.FromResult(session));

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Query.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, sessionRepository.Object, identityQuery.Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);

        sessionRepository.Verify(m => m.RenewAsync(session, CancellationToken.None), Times.Once);
    }
}