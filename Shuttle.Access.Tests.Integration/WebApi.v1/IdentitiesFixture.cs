using System.Net;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class IdentitiesFixture
{
    private static SqlServer.Models.Identity CreateIdentity()
    {
        var now = DateTimeOffset.UtcNow;

        return new()
        {
            Id = Guid.NewGuid(),
            Name = "name",
            DateRegistered = now,
            DateActivated = now,
            GeneratedPassword = "generated-password",
            RegisteredBy = "system",
            IdentityRoles =
            [
                new()
                {
                    RoleId = Guid.NewGuid(),
                    Role = new()
                    {
                        Name = "role"
                    }
                }
            ]
        };
    }

    [Test]
    public async Task Should_be_able_to_activate_identity()
    {
        var identity = CreateIdentity();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<SqlServer.Models.Identity.Specification>(), CancellationToken.None)).Returns(
            Task.FromResult(
                new List<SqlServer.Models.Identity>
                {
                    identity
                }.AsEnumerable()));

        var response = await factory.GetAccessClient().Identities.ActivateAsync(new()
        {
            Name = "known"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.ServiceBus.Verify(m => m.SendAsync(It.IsAny<ActivateIdentity>(), null), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_change_password_async()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.ServiceBus.Setup(m => m.SendAsync(It.Is<ChangePassword>(message => message.Token.Equals(token)), null)).Verifiable();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RequestMessage<ChangePassword>>(), CancellationToken.None)).Verifiable();

        var response = await factory.GetAccessClient().Identities.ChangePasswordAsync(new()
        {
            NewPassword = "new-password",
            Token = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ChangePassword>(), CancellationToken.None), Times.Never);
        factory.ServiceBus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_be_able_to_delete_identity_async()
    {
        var id = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.ServiceBus.Setup(m => m.SendAsync(It.Is<RemoveIdentity>(message => message.Id.Equals(id)), null)).Verifiable();

        var response = await factory.GetAccessClient().Identities.DeleteAsync(id);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.ServiceBus.VerifyAll();
    }

    [Test]
    public async Task Should_be_able_to_get_identity_by_value_async()
    {
        var identity = CreateIdentity();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<SqlServer.Models.Identity.Specification>(), CancellationToken.None)).Returns(
            Task.FromResult(new List<SqlServer.Models.Identity>
            {
                identity
            }.AsEnumerable()));

        var response = await factory.GetAccessClient().Identities.GetAsync("some-value");

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);
        Assert.That(response.Content!.Id, Is.EqualTo(identity.Id));
        Assert.That(response.Content.Name, Is.EqualTo(identity.Name));
        Assert.That(response.Content.DateRegistered, Is.EqualTo(identity.DateRegistered));
        Assert.That(response.Content.DateActivated, Is.EqualTo(identity.DateActivated));
        Assert.That(response.Content.GeneratedPassword, Is.EqualTo(identity.GeneratedPassword));
        Assert.That(response.Content.RegisteredBy, Is.EqualTo(identity.RegisteredBy));
        Assert.That(response.Content.Roles.Find(item => item.Id == identity.IdentityRoles.First().RoleId), Is.Not.Null);
    }

    [Test]
    public async Task Should_be_able_to_get_password_reset_token_async()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m =>
                m.SendAsync(It.IsAny<RequestResponseMessage<GetPasswordResetToken, Guid>>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((message, _) =>
            {
                ((RequestResponseMessage<GetPasswordResetToken, Guid>)message).WithResponse(token);
            });

        var response = await factory.GetAccessClient().Identities.GetPasswordResetTokenAsync("identity");

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content, Is.EqualTo(token));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<RequestResponseMessage<GetPasswordResetToken, Guid>>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_get_role_status_async()
    {
        var activeRoleId = Guid.NewGuid();
        var inactiveRoleId = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.RoleIdsAsync(It.IsAny<SqlServer.Models.Identity.Specification>(), CancellationToken.None)).Returns(
            Task.FromResult(
                new List<Guid>
                {
                    activeRoleId
                }.AsEnumerable()));

        var response = await factory.GetAccessClient().Identities.RoleAvailabilityAsync(Guid.NewGuid(), new()
        {
            Values =
            [
                activeRoleId,
                inactiveRoleId
            ]
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);

        Assert.That(response.Content!.Count, Is.EqualTo(2));

        var identityRoleStatus = response.Content.Find(item => item.Id == activeRoleId);

        Assert.That(identityRoleStatus, Is.Not.Null);
        Assert.That(identityRoleStatus!.Active, Is.True);

        identityRoleStatus = response.Content.Find(item => item.Id == inactiveRoleId);

        Assert.That(identityRoleStatus, Is.Not.Null);
        Assert.That(identityRoleStatus!.Active, Is.False);
    }

    [Test]
    public async Task Should_be_able_to_register_identity_async()
    {
        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RequestIdentityRegistration>(), CancellationToken.None))
            .Callback<object, CancellationToken>((message, _) =>
            {
                ((RequestIdentityRegistration)message).Allowed("test", true);
            });

        var response = await factory.GetAccessClient().Identities.RegisterAsync(new()
        {
            Name = "identity"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<RequestIdentityRegistration>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_reset_password_async()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.ServiceBus.Setup(m => m.SendAsync(It.Is<ResetPassword>(message => message.PasswordResetToken.Equals(token)), null)).Verifiable();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RequestMessage<ResetPassword>>(), CancellationToken.None)).Verifiable();

        var response = await factory.GetAccessClient().Identities.ResetPasswordAsync(new()
        {
            Name = "identity",
            Password = "password",
            PasswordResetToken = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ResetPassword>(), CancellationToken.None), Times.Never);
        factory.ServiceBus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_be_able_to_search_identities_async()
    {
        var identity = CreateIdentity();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<SqlServer.Models.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(
            new List<SqlServer.Models.Identity>
            {
                identity
            }.AsEnumerable()));

        var response = await factory.GetAccessClient().Identities.SearchAsync(new());

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);

        Assert.That(response.Content!.Count, Is.EqualTo(1));

        var responseIdentity = response.Content[0];

        Assert.That(responseIdentity.Id, Is.EqualTo(identity.Id));
        Assert.That(responseIdentity.Name, Is.EqualTo(identity.Name));
        Assert.That(responseIdentity.DateRegistered, Is.EqualTo(identity.DateRegistered));
        Assert.That(responseIdentity.DateActivated, Is.EqualTo(identity.DateActivated));
        Assert.That(responseIdentity.GeneratedPassword, Is.EqualTo(identity.GeneratedPassword));
        Assert.That(responseIdentity.RegisteredBy, Is.EqualTo(identity.RegisteredBy));
        Assert.That(responseIdentity.Roles.Find(item => item.Id == identity.IdentityRoles.First().RoleId), Is.Not.Null);
    }

    [Test]
    public async Task Should_be_able_to_set_identity_role_status_async()
    {
        var roleId = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.ServiceBus.Setup(m => m.SendAsync(It.Is<SetIdentityRole>(message => message.RoleId.Equals(roleId)), null)).Verifiable();
        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RequestMessage<SetIdentityRole>>(), CancellationToken.None)).Verifiable();

        var response = await factory.GetAccessClient().Identities.SetRoleAsync(Guid.NewGuid(), roleId, new()
        {
            Active = true
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.ServiceBus.VerifyAll();
        factory.Mediator.VerifyAll();
    }

    [Test]
    public async Task Should_not_be_able_to_activate_unknown_identity()
    {
        var factory = new FixtureWebApplicationFactory();

        var response = await factory.GetAccessClient().Identities.ActivateAsync(new()
        {
            Name = "unknown"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Should_not_be_able_to_change_password_when_mediator_call_fails()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RequestMessage<ChangePassword>>(), CancellationToken.None))
            .Callback<object, CancellationToken>((message, _) =>
            {
                ((RequestMessage<ChangePassword>)message).Failed("reason");
            });

        var response = await factory.GetAccessClient().Identities.ChangePasswordAsync(new()
        {
            NewPassword = "new-password",
            Token = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ChangePassword>(), CancellationToken.None), Times.Never);
        factory.ServiceBus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_not_be_able_to_change_password_when_no_session_token_is_provided()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        var response = await factory.GetAccessClient(httpClient =>
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
        }).Identities.ChangePasswordAsync(new()
        {
            NewPassword = "new-password",
            Token = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ChangePassword>(), CancellationToken.None), Times.Never);
        factory.ServiceBus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_not_be_able_to_get_password_reset_token_when_mediator_call_fails_async()
    {
        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RequestResponseMessage<GetPasswordResetToken, Guid>>(), CancellationToken.None))
            .Callback<object, CancellationToken>((message, _) =>
            {
                ((RequestResponseMessage<GetPasswordResetToken, Guid>)message).Failed("reason");
            });

        var response = await factory.GetAccessClient().Identities.GetPasswordResetTokenAsync("identity");

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<RequestResponseMessage<GetPasswordResetToken, Guid>>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Should_not_be_able_to_reset_password_when_mediator_call_fails()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RequestMessage<ResetPassword>>(), CancellationToken.None))
            .Callback<object, CancellationToken>((message, _) =>
            {
                ((RequestMessage<ResetPassword>)message).Failed("reason");
            });

        var response = await factory.GetAccessClient().Identities.ResetPasswordAsync(new()
        {
            Name = "identity",
            Password = "password",
            PasswordResetToken = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ResetPassword>(), CancellationToken.None), Times.Never);
        factory.ServiceBus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_not_be_able_to_reset_password_when_no_session_token_is_provided_async()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        var response = await factory.GetAccessClient(httpClient =>
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
        }).Identities.ResetPasswordAsync(new()
        {
            Name = "identity",
            Password = "password",
            PasswordResetToken = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ResetPassword>(), CancellationToken.None), Times.Never);
        factory.ServiceBus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_not_be_able_to_set_identity_role_status_when_mediator_call_fails_async()
    {
        var roleId = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<RequestMessage<SetIdentityRole>>(), CancellationToken.None))
            .Callback<object, CancellationToken>((message, _) =>
            {
                ((RequestMessage<SetIdentityRole>)message).Failed("reason");
            });

        var response = await factory.GetAccessClient().Identities.SetRoleAsync(Guid.NewGuid(), roleId, new()
        {
            IdentityId = Guid.NewGuid()
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        factory.ServiceBus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }
}