using System.Net;
using Moq;
using NUnit.Framework;
using Shuttle.Access.Application;
using ActivateIdentity = Shuttle.Access.Messages.v1.ActivateIdentity;
using RegisterIdentity = Shuttle.Access.Messages.v1.RegisterIdentity;
using RemoveIdentity = Shuttle.Access.Messages.v1.RemoveIdentity;
using SetIdentityRoleStatus = Shuttle.Access.Messages.v1.SetIdentityRoleStatus;
using SetIdentityTenantStatus = Shuttle.Access.Messages.v1.SetIdentityTenantStatus;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

public class IdentitiesFixture
{
    private static Query.Identity CreateIdentity()
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
            Roles =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "role"
                }
            ]
        };
    }

    [Test]
    public async Task Should_be_able_to_activate_identity()
    {
        var identity = CreateIdentity();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).Returns(
            Task.FromResult(
                new List<Query.Identity>
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

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<ActivateIdentity>(), null), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_change_password_async()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<ChangePassword>(), CancellationToken.None)).Verifiable();

        var response = await factory.GetAccessClient().Identities.ChangePasswordAsync(new()
        {
            NewPassword = "new-password",
            Token = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ChangePassword>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_delete_identity_async()
    {
        var id = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Bus.Setup(m => m.SendAsync(It.Is<RemoveIdentity>(message => message.Id.Equals(id)), null)).Verifiable();

        var response = await factory.GetAccessClient().Identities.DeleteAsync(id);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.Bus.VerifyAll();
    }

    [Test]
    public async Task Should_be_able_to_delete_identity_directly_async()
    {
        var id = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory(useMessaging: false);

        factory.Mediator.Setup(m => m.SendAsync(It.Is<Application.RemoveIdentity>(message => message.Id.Equals(id)), It.IsAny<CancellationToken>())).Verifiable();

        var response = await factory.GetAccessClient().Identities.DeleteAsync(id);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.Mediator.VerifyAll();
        factory.Bus.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_be_able_to_get_identity_by_id_async()
    {
        var identity = CreateIdentity();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), CancellationToken.None)).Returns(
            Task.FromResult(new List<Query.Identity>
            {
                identity
            }.AsEnumerable()));

        var response = await factory.GetAccessClient().Identities.GetAsync(identity.Id);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);
        Assert.That(response.Content!.Id, Is.EqualTo(identity.Id));
        Assert.That(response.Content.Name, Is.EqualTo(identity.Name));
        Assert.That(response.Content.DateRegistered, Is.EqualTo(identity.DateRegistered));
        Assert.That(response.Content.DateActivated, Is.EqualTo(identity.DateActivated));
        Assert.That(response.Content.GeneratedPassword, Is.EqualTo(identity.GeneratedPassword));
        Assert.That(response.Content.RegisteredBy, Is.EqualTo(identity.RegisteredBy));
        Assert.That(response.Content.Roles.Find(item => item.Id == identity.Roles.First().Id), Is.Not.Null);
    }

    [Test]
    public async Task Should_be_able_to_get_password_reset_token_async()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m =>
                m.SendAsync(It.IsAny<GetPasswordResetToken>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((message, _) =>
            {
                ((GetPasswordResetToken)message).WithPasswordResetToken(token);
            });

        var response = await factory.GetAccessClient().Identities.GetPasswordResetTokenAsync(Guid.NewGuid());

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content, Is.EqualTo(token));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<GetPasswordResetToken>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_get_role_status_async()
    {
        var activeRoleId = Guid.NewGuid();
        var inactiveRoleId = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.RoleIdsAsync(It.IsAny<Query.Identity.Specification>(), CancellationToken.None)).Returns(
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

        factory.Bus.Setup(m => m.SendAsync(It.IsAny<RegisterIdentity>(), null, It.IsAny<CancellationToken>()));

        var response = await factory.GetAccessClient().Identities.RegisterAsync(new()
        {
            Name = "identity"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<RegisterIdentity>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_reset_password_async()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<ResetPassword>(), CancellationToken.None)).Verifiable();

        var response = await factory.GetAccessClient().Identities.ResetPasswordAsync(new()
        {
            Name = "identity",
            Password = "password",
            PasswordResetToken = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ResetPassword>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Should_be_able_to_search_identities_async()
    {
        var identity = CreateIdentity();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), CancellationToken.None)).Returns(Task.FromResult(
            new List<Query.Identity>
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
        Assert.That(responseIdentity.Roles.Find(item => item.Id == identity.Roles.First().Id), Is.Not.Null);
    }

    [Test]
    public async Task Should_be_able_to_set_identity_role_status_async()
    {
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var identity = CreateIdentity();

        identity.Tenants =
        [
            new()
            {
                Id = tenantId
            }
        ];

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
            new List<Query.Identity>
            {
                identity
            }.AsEnumerable()));

        factory.RoleQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Role.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = roleId,
                TenantId = tenantId
            }
        ]);

        factory.Bus.Setup(m => m.SendAsync(It.Is<SetIdentityRoleStatus>(message => message.RoleId.Equals(roleId)), null, It.IsAny<CancellationToken>())).Verifiable();

        var response = await factory.GetAccessClient().Identities.SetRoleStatusAsync(identity.Id, roleId, new()
        {
            Active = true
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.Bus.VerifyAll();
    }

    [Test]
    public async Task Should_be_able_to_set_identity_tenant_status_async()
    {
        var identity = CreateIdentity();
        var tenantId = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([identity]);

        factory.TenantQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Tenant.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = tenantId,
                Name = "tenant",
                MaximumIdentities = 0
            }
        ]);

        factory.Bus.Setup(m => m.SendAsync(It.Is<SetIdentityTenantStatus>(message => message.TenantId.Equals(tenantId)), null, It.IsAny<CancellationToken>())).Verifiable();

        var response = await factory.GetAccessClient().Identities.SetTenantAsync(identity.Id, tenantId, new()
        {
            Active = true
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.Bus.VerifyAll();
    }

    [Test]
    public async Task Should_not_be_able_to_set_identity_tenant_status_when_tenant_is_at_maximum_identities_async()
    {
        var identity = CreateIdentity();
        var tenantId = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([identity]);

        factory.IdentityQuery.Setup(m => m.CountAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync(2);

        factory.TenantQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Tenant.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = tenantId,
                Name = "tenant",
                MaximumIdentities = 2
            }
        ]);

        var response = await factory.GetAccessClient().Identities.SetTenantAsync(identity.Id, tenantId, new()
        {
            Active = true
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<SetIdentityTenantStatus>(), null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Should_be_able_to_set_identity_tenant_status_when_identity_is_already_in_the_tenant_at_maximum_identities_async()
    {
        var tenantId = Guid.NewGuid();

        var identity = CreateIdentity();

        identity.Tenants = [new() { Id = tenantId }];

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([identity]);

        factory.TenantQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Tenant.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = tenantId,
                Name = "tenant",
                MaximumIdentities = 1
            }
        ]);

        factory.Bus.Setup(m => m.SendAsync(It.Is<SetIdentityTenantStatus>(message => message.TenantId.Equals(tenantId)), null, It.IsAny<CancellationToken>())).Verifiable();

        var response = await factory.GetAccessClient().Identities.SetTenantAsync(identity.Id, tenantId, new()
        {
            Active = true
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.Bus.VerifyAll();

        factory.IdentityQuery.Verify(m => m.CountAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>()), Times.Never);
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

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<ChangePassword>(), CancellationToken.None))
            .Callback<object, CancellationToken>((_, _) => throw new ApplicationException("reason"));

        var response = await factory.GetAccessClient().Identities.ChangePasswordAsync(new()
        {
            NewPassword = "new-password",
            Token = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ChangePassword>(), CancellationToken.None), Times.Once);
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
        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_not_be_able_to_deactivate_last_administrator_async()
    {
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var identity = CreateIdentity();

        identity.Tenants =
        [
            new()
            {
                Id = tenantId
            }
        ];

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
            new List<Query.Identity>
            {
                identity
            }.AsEnumerable()));

        factory.RoleQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Role.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = roleId,
                TenantId = tenantId
            }
        ]);

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<ReviewIdentityRoleRemoval>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((message, _) => ((ReviewIdentityRoleRemoval)message).LastAdministrator());

        var response = await factory.GetAccessClient().Identities.SetRoleStatusAsync(Guid.NewGuid(), roleId, new()
        {
            Active = false
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }

    [Test]
    public async Task Should_not_be_able_to_get_password_reset_token_when_mediator_call_fails_async()
    {
        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<GetPasswordResetToken>(), CancellationToken.None))
            .Callback<object, CancellationToken>((_, _) => throw new ApplicationException("reason"));

        var response = await factory.GetAccessClient().Identities.GetPasswordResetTokenAsync(Guid.NewGuid());

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<GetPasswordResetToken>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Should_not_be_able_to_reset_password_when_mediator_call_fails()
    {
        var token = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.Mediator.Setup(m => m.SendAsync(It.IsAny<ResetPassword>(), CancellationToken.None))
            .Callback<object, CancellationToken>((_, _) => throw new ApplicationException("reason"));

        var response = await factory.GetAccessClient().Identities.ResetPasswordAsync(new()
        {
            Name = "identity",
            Password = "password",
            PasswordResetToken = token
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.False);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

        factory.Mediator.Verify(m => m.SendAsync(It.IsAny<ResetPassword>(), CancellationToken.None), Times.Once);
    }

    [Test]
    public async Task Should_not_be_able_to_reset_password_when_no_session_token_is_provided_async()
    {
        var token = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var identity = CreateIdentity();

        identity.Tenants =
        [
            new()
            {
                Id = tenantId
            }
        ];

        var factory = new FixtureWebApplicationFactory();

        factory.IdentityQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Identity.Specification>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(
            new List<Query.Identity>
            {
                identity
            }.AsEnumerable()));

        factory.RoleQuery.Setup(m => m.SearchAsync(It.IsAny<Query.Role.Specification>(), It.IsAny<CancellationToken>())).ReturnsAsync([
            new()
            {
                Id = roleId,
                TenantId = tenantId
            }
        ]);

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
        factory.Bus.Verify(m => m.SendAsync(It.IsAny<object>(), null), Times.Never);
    }
}