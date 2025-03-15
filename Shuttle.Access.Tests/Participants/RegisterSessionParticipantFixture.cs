using System;
using System.Linq;
using System.Text.RegularExpressions;
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
        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, new Mock<ISessionRepository>().Object, new Mock<IIdentityQuery>().Object, new Mock<ISessionTokenExchangeRepository>().Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Exception);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_a_password()
    {
        var message = new RegisterSession(IdentityName).UsePassword("some_password");
        var authenticationService = new Mock<IAuthenticationService>();

        authenticationService.Setup(m => m.AuthenticateAsync(IdentityName, "some_password", CancellationToken.None)).ReturnsAsync(new AuthenticationResult(true, IdentityName));

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), authenticationService.Object, new Mock<IAuthorizationService>().Object, new Mock<ISessionRepository>().Object, identityQuery.Object, new Mock<ISessionTokenExchangeRepository>().Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_a_password_for_an_application()
    {
        var message = new RegisterSession(IdentityName).UsePassword("some_password").WithKnownApplicationOptions(new() { SessionTokenExchangeUrl = "http://localhost" });
        var authenticationService = new Mock<IAuthenticationService>();

        authenticationService.Setup(m => m.AuthenticateAsync(IdentityName, "some_password", CancellationToken.None)).ReturnsAsync(new AuthenticationResult(true, IdentityName));

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var sessionTokenExchangeRepository = new Mock<ISessionTokenExchangeRepository>();

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), authenticationService.Object, new Mock<IAuthorizationService>().Object, new Mock<ISessionRepository>().Object, identityQuery.Object, sessionTokenExchangeRepository.Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
        Assert.That(new Regex("http://localhost/[0-9a-fA-F-]{36}").Match(message.SessionTokenExchangeUrl).Success, Is.True);

        sessionTokenExchangeRepository.Verify(m => m.SaveAsync(It.IsAny<SessionTokenExchange>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_delegation()
    {
        var sessionToken = Guid.NewGuid();
        var message = new RegisterSession(IdentityName).UseDelegation(sessionToken);
        var sessionRepository = new Mock<ISessionRepository>();
        var sessionQuery = new Mock<ISessionQuery>();

        var now = DateTimeOffset.UtcNow;
        var session = new Session(sessionToken, Guid.NewGuid(), IdentityName, now, now.AddMinutes(1))
            .AddPermission(AccessPermissions.Sessions.Register);

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Session.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Session { Token = sessionToken } }.AsEnumerable()));

        sessionRepository.Setup(m => m.FindAsync(sessionToken, CancellationToken.None)).Returns(Task.FromResult(session)!);

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, sessionRepository.Object, identityQuery.Object, new Mock<ISessionTokenExchangeRepository>().Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_a_token()
    {
        var sessionToken = Guid.NewGuid();
        var message = new RegisterSession(IdentityName).UseToken(sessionToken);

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var sessionRepository = new Mock<ISessionRepository>();
        var sessionQuery = new Mock<ISessionQuery>();

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Session.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Session { Token = sessionToken } }.AsEnumerable()));

        sessionRepository.Setup(m => m.FindAsync(sessionToken, CancellationToken.None)).Returns(Task.FromResult(new Session(sessionToken, Guid.NewGuid(), IdentityName, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(1)))!);

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, sessionRepository.Object, identityQuery.Object, new Mock<ISessionTokenExchangeRepository>().Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_direct_registration()
    {
        var message = new RegisterSession(IdentityName).UseDirect();

        var identityQuery = new Mock<IIdentityQuery>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, new Mock<ISessionRepository>().Object, identityQuery.Object, new Mock<ISessionTokenExchangeRepository>().Object);

        Assert.That(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_new_session()
    {
        var message = new RegisterSession(IdentityName).UseDirect();

        var identityQuery = new Mock<IIdentityQuery>();
        var sessionRepository = new Mock<ISessionRepository>();

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, sessionRepository.Object, identityQuery.Object, new Mock<ISessionTokenExchangeRepository>().Object);

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
        var sessionQuery = new Mock<ISessionQuery>();

        var now = DateTimeOffset.UtcNow;
        var sessionToken = Guid.NewGuid();
        var session = new Session(sessionToken, Guid.NewGuid(), IdentityName, now, now.AddMinutes(-5));

        sessionQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Session.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Session { Token = sessionToken } }.AsEnumerable()));

        sessionRepository.Setup(m => m.FindAsync(sessionToken, CancellationToken.None)).Returns(Task.FromResult(session)!);

        identityQuery.Setup(m => m.SearchAsync(It.IsAny<DataAccess.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(new[] { new Messages.v1.Identity() }.AsEnumerable()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new Mock<IAuthorizationService>().Object, sessionRepository.Object, identityQuery.Object, new Mock<ISessionTokenExchangeRepository>().Object);

        Assert.ThatAsync(async () => await participant.ProcessMessageAsync(new ParticipantContext<RegisterSession>(message, CancellationToken.None)), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);

        sessionRepository.Verify(m => m.SaveAsync(session, CancellationToken.None), Times.Once);

        Assert.That(session.Token, Is.Not.EqualTo(sessionToken));
    }
}