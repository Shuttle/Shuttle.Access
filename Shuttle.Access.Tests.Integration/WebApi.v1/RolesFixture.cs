using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;

namespace Shuttle.Access.Tests.Integration.WebApi.v1;

[TestFixture]
public class RolesFixture
{
    private static Messages.v1.Role CreateRole()
    {
        return new Messages.v1.Role
        {
            Id = Guid.NewGuid(),
            Name = "name",
            Permissions = new List<Messages.v1.Role.Permission>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "system://permission-a"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "system://permission-b"
                }
            }
        };
    }

    [Test]
    public async Task Should_be_able_to_get_all_roles_async()
    {
        var factory = new FixtureWebApplicationFactory();

        var role = CreateRole();

        factory.RoleQuery.Setup(m => m.SearchAsync(It.IsAny<RoleSpecification>(), default)).Returns(
            Task.FromResult(new List<Messages.v1.Role>
            {
                role
            }.AsEnumerable()));

        var client = factory.GetAccessClient();

        var response = await client.Roles.GetAsync();

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);
        Assert.That(response.Content.Count, Is.EqualTo(1));

        var responseRole = response.Content[0];

        Assert.That(responseRole.Id, Is.EqualTo(role.Id));
        Assert.That(responseRole.Name, Is.EqualTo(role.Name));
        Assert.That(responseRole.Permissions.Find(item => item.Id == role.Permissions[0].Id), Is.Not.Null);
        Assert.That(responseRole.Permissions.Find(item => item.Id == role.Permissions[1].Id), Is.Not.Null);
    }

    [Test]
    public async Task Should_be_able_to_get_role_by_value_async()
    {
        var factory = new FixtureWebApplicationFactory();

        var role = CreateRole();

        factory.RoleQuery.Setup(m => m.SearchAsync(It.IsAny<RoleSpecification>(), default)).Returns(
            Task.FromResult(new List<Messages.v1.Role>
            {
                role
            }.AsEnumerable()));

        var client = factory.GetAccessClient();

        var response = await client.Roles.GetAsync("some-value");

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);
        Assert.That(response.Content.Id, Is.EqualTo(role.Id));
        Assert.That(response.Content.Name, Is.EqualTo(role.Name));
        Assert.That(response.Content.Permissions.Find(item => item.Id == role.Permissions[0].Id), Is.Not.Null);
        Assert.That(response.Content.Permissions.Find(item => item.Id == role.Permissions[1].Id), Is.Not.Null);
    }

    [Test]
    public async Task Should_be_able_to_delete_role_async()
    {
        var id = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.ServiceBus.Setup(m => m.SendAsync(It.Is<RemoveRole>(message => message.Id.Equals(id)), null)).Verifiable();

        var response = await factory.GetAccessClient().Roles.DeleteAsync(id);

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);

        factory.ServiceBus.VerifyAll();
    }

    [Test]
    public async Task Should_be_able_to_set_role_async()
    {
        var permissionId = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.ServiceBus.Setup(m =>
                m.SendAsync(It.Is<SetRolePermission>(message => message.PermissionId.Equals(permissionId)), null))
            .Verifiable();

        var response = await factory.GetAccessClient().Roles.SetPermissionAsync(Guid.NewGuid(), new SetRolePermission
        {
            PermissionId = permissionId,
            Active = true
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.ServiceBus.VerifyAll();
    }

    [Test]
    public async Task Should_be_able_to_get_role_async()
    {
        var activePermissionId = Guid.NewGuid();
        var inactivePermissionId = Guid.NewGuid();

        var factory = new FixtureWebApplicationFactory();

        factory.RoleQuery.Setup(m => m.PermissionsAsync(It.IsAny<RoleSpecification>(), default)).Returns(
            Task.FromResult(new List<Messages.v1.Permission>
            {
                new()
                {
                    Id = activePermissionId,
                    Name = "system://permission-a"
                }
            }.AsEnumerable()));

        var response = await factory.GetAccessClient().Roles.PermissionAvailabilityAsync(Guid.NewGuid(), new Identifiers<Guid>
        {
            Values = new List<Guid>
            {
                activePermissionId,
                inactivePermissionId
            }
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.Content, Is.Not.Null);

        Assert.That(response.Content.Count, Is.EqualTo(2));

        var permission = response.Content.Find(item => item.Id == activePermissionId);

        Assert.That(permission, Is.Not.Null);
        Assert.That(permission.Active, Is.True);

        permission = response.Content.Find(item => item.Id == inactivePermissionId);

        Assert.That(permission, Is.Not.Null);
        Assert.That(permission.Active, Is.False);
    }

    [Test]
    public async Task Should_be_able_to_register_role_async()
    {
        var factory = new FixtureWebApplicationFactory();

        var response = await factory.GetAccessClient().Roles.RegisterAsync(new RegisterRole
        {
            Name = "role"
        });

        Assert.That(response, Is.Not.Null);
        Assert.That(response.IsSuccessStatusCode, Is.True);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        factory.ServiceBus.Verify(m => m.SendAsync(It.IsAny<RegisterRole>(), null), Times.Once);
    }
}