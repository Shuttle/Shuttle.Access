using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;

namespace Shuttle.Access.Tests.Participants;

[TestFixture]
public class RegisterSessionParticipantFixture
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private const string IdentityName = "some-identity";

    [Test]
    public void Should_not_be_able_to_register_a_session_when_no_registration_type_has_been_set()
    {
        var message = new RegisterSession(IdentityName);
        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new HashingService(), new Mock<ISessionQuery>().Object, new Mock<IIdentityQuery>().Object);

        Assert.That(async () => await participant.HandleAsync(message, It.IsAny<CancellationToken>()), Throws.Exception);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_a_password()
    {
        var message = new RegisterSession(IdentityName).UsePassword("some_password");
        var authenticationService = new Mock<IAuthenticationService>();

        authenticationService.Setup(m => m.AuthenticateAsync(IdentityName, "some_password", It.IsAny<CancellationToken>())).ReturnsAsync(new AuthenticationResult(true, IdentityName));

        var identityQuery = MockIdentitySearchAsync();
        var sessionQuery = MockSessionSearchAsync(out _);

        identityQuery.Setup(m => m.IdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(ValueTask.FromResult(Guid.NewGuid()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), authenticationService.Object, new HashingService(), sessionQuery.Object, identityQuery.Object);

        Assert.That(async () => await participant.HandleAsync(message, It.IsAny<CancellationToken>()), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_delegation()
    {
        var sessionToken = Guid.NewGuid();
        var message = new RegisterSession(IdentityName).UseDelegation(_tenantId, sessionToken);

        var identityQuery = MockIdentitySearchAsync();
        var sessionQuery = MockSessionSearchAsync(out var session);

        session.Permissions = [new()
        {
            Name = AccessPermissions.Sessions.Register
        }];

        identityQuery.Setup(m => m.IdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(ValueTask.FromResult(Guid.NewGuid()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new HashingService(), sessionQuery.Object, identityQuery.Object);

        Assert.That(async () => await participant.HandleAsync(message, It.IsAny<CancellationToken>()), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_a_token()
    {
        var hashingService = new HashingService();
        var sessionToken = Guid.NewGuid();
        var message = new RegisterSession(IdentityName).UseSessionToken(sessionToken);

        var identityQuery = MockIdentitySearchAsync();
        var sessionQuery = MockSessionSearchAsync(out _);

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, hashingService, sessionQuery.Object, identityQuery.Object);

        Assert.That(async () => await participant.HandleAsync(message, It.IsAny<CancellationToken>()), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    private Mock<IIdentityQuery> MockIdentitySearchAsync()
    {
        var result = new Mock<IIdentityQuery>();

        result.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new[] { new Query.Identity
        {
            Id = Guid.NewGuid(),
            Tenants = [new()
            {
                Id = _tenantId,
                Status = TenantStatus.Active
            }]
        } }.AsEnumerable()));

        return result;
    }
    private Mock<ISessionQuery> MockSessionSearchAsync(out Query.Session session)
    {
        var result = new Mock<ISessionQuery>();
        var now = DateTime.UtcNow;

        var instance = new Query.Session
        {
            Id = Guid.NewGuid(),
            IdentityId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            DateRegistered = now,
            ExpiryDate = now.AddMinutes(1)
        };

        session = instance;

        result.Setup(m => m.SearchAsync(It.IsAny<Query.Session.Specification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new[] { instance }.AsEnumerable()));

        return result;
    }

    [Test]
    public void Should_be_able_to_register_a_session_using_direct_registration()
    {
        var message = new RegisterSession(IdentityName).UseDirect();

        var identityQuery = MockIdentitySearchAsync();
        var sessionQuery = MockSessionSearchAsync(out _);

        identityQuery.Setup(m => m.IdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(ValueTask.FromResult(Guid.NewGuid()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new HashingService(), sessionQuery.Object, identityQuery.Object);

        Assert.That(async () => await participant.HandleAsync(message, It.IsAny<CancellationToken>()), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);
    }

    [Test]
    public void Should_be_able_to_register_a_new_session()
    {
        var message = new RegisterSession(IdentityName).UseDirect();

        var identityQuery = MockIdentitySearchAsync();
        var sessionQuery = MockSessionSearchAsync(out _);

        identityQuery.Setup(m => m.IdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(ValueTask.FromResult(Guid.NewGuid()));

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new HashingService(), sessionQuery.Object, identityQuery.Object);

        Assert.That(async () => await participant.HandleAsync(message, It.IsAny<CancellationToken>()), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);

        sessionQuery.Verify(m => m.SaveAsync(It.IsAny<Query.Session>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Should_be_able_to_renew_a_session()
    {
        var message = new RegisterSession(IdentityName)
            .UseDirect()
            .WithTenantId(_tenantId);

        var sessionToken = new HashingService().Sha256($"{Guid.NewGuid():D}");

        var identityQuery = MockIdentitySearchAsync();
        var sessionQuery = MockSessionSearchAsync(out var session);

        session.TokenHash = sessionToken;

        var participant = new RegisterSessionParticipant(Options.Create(new AccessOptions()), new Mock<IAuthenticationService>().Object, new HashingService(), sessionQuery.Object, identityQuery.Object);

        Assert.ThatAsync(async () => await participant.HandleAsync(message, It.IsAny<CancellationToken>()), Throws.Nothing);
        Assert.That(message.HasSession, Is.True);

        sessionQuery.Verify(m => m.SaveAsync(It.IsAny<Query.Session>(), It.IsAny<CancellationToken>()), Times.Once);

        Assert.That(message.Session!.TokenHash, Is.EqualTo(sessionToken));
    }
}