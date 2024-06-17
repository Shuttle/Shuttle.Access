using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
using Shuttle.Esb;

namespace Shuttle.Access.Tests.Integration.WebApi.v1
{
    public class RolesFixture
    {
        [Test]
        public async Task Should_be_able_to_get_all_roles_async()
        {
            var factory = new FixtureWebApplicationFactory();
            
            var role = CreateRole();

            factory.RoleQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Role.Specification>())).Returns(
                new List<Access.DataAccess.Query.Role>
                {
                    role
                });

            
                var client = factory.CreateClient();

                var response = await client.GetFromJsonAsync<List<Shuttle.Access.DataAccess.Query.Role>>("/v1/roles");

                Assert.That(response, Is.Not.Null);
                Assert.That(response.Count, Is.EqualTo(1));

                var responseRole = response[0];

                Assert.That(responseRole.Id, Is.EqualTo(role.Id));
                Assert.That(responseRole.Name, Is.EqualTo(role.Name));
                Assert.That(responseRole.Permissions.Find(item => item.Id == role.Permissions[0].Id), Is.Not.Null);
                Assert.That(responseRole.Permissions.Find(item => item.Id == role.Permissions[1].Id), Is.Not.Null);
        }

        private static Access.DataAccess.Query.Role CreateRole()
        {
            return new Access.DataAccess.Query.Role
            {
                Id = Guid.NewGuid(),
                Name = "name",
                Permissions = new List<Access.DataAccess.Query.Role.Permission>
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

        //[Test]
        //public async Task Should_be_able_to_get_role_by_value_async()
        //{
        //    var roleQuery = new Mock<IRoleQuery>();

        //    var role = CreateRole();

        //    roleQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Role.Specification>())).Returns(
        //        new List<Access.DataAccess.Query.Role>
        //        {
        //            role
        //        });

        //    using (var httpClient = Factory.WithWebHostBuilder(builder =>
        //           {
        //               builder.ConfigureTestServices(services =>
        //               {
        //                   services.AddSingleton(roleQuery.Object);
        //               });
        //           }).CreateClient())
        //    {
        //        var client = GetClient(httpClient);

        //        var response = await client.Roles.GetAsync("some-value");

        //        Assert.That(response, Is.Not.Null);
        //        Assert.That(response.IsSuccessStatusCode, Is.True);
        //        Assert.That(response.Content, Is.Not.Null);
        //        Assert.That(response.Content.Id, Is.EqualTo(role.Id));
        //        Assert.That(response.Content.Name, Is.EqualTo(role.Name));
        //        Assert.That(response.Content.Permissions.Find(item => item.Id == role.Permissions[0].Id), Is.Not.Null);
        //        Assert.That(response.Content.Permissions.Find(item => item.Id == role.Permissions[1].Id), Is.Not.Null);
        //    }
        //}

        //[Test]
        //public async Task Should_be_able_to_delete_role_async()
        //{
        //    var id = Guid.NewGuid();
        //    var serviceBus = new Mock<IServiceBus>();

        //    serviceBus.Setup(m => m.Send(It.Is<RemoveRole>(message => message.Id.Equals(id)), null)).Verifiable();

        //    using (var httpClient = Factory.WithWebHostBuilder(builder =>
        //           {
        //               builder.ConfigureTestServices(services =>
        //               {
        //                   services.AddSingleton(new Mock<IRoleQuery>().Object);
        //                   services.AddSingleton(serviceBus.Object);
        //               });
        //           }).CreateDefaultClient())
        //    {
        //        var client = await GetClient(httpClient).RegisterSessionAsync();

        //        var response = await client.Roles.DeleteAsync(id);

        //        Assert.That(response, Is.Not.Null);
        //        Assert.That(response.IsSuccessStatusCode, Is.True);

        //        serviceBus.VerifyAll();
        //    }
        //}

        //[Test]
        //public async Task Should_be_able_to_set_role_async()
        //{
        //    var permissionId = Guid.NewGuid();

        //    var serviceBus = new Mock<IServiceBus>();

        //    serviceBus.Setup(m =>
        //            m.Send(It.Is<SetRolePermission>(message => message.PermissionId.Equals(permissionId)), null))
        //        .Verifiable();

        //    using (var httpClient = Factory.WithWebHostBuilder(builder =>
        //           {
        //               builder.ConfigureTestServices(services =>
        //               {
        //                   services.AddSingleton(new Mock<IRoleQuery>().Object);
        //                   services.AddSingleton(serviceBus.Object);
        //               });
        //           }).CreateDefaultClient())
        //    {
        //        var client = await GetClient(httpClient).RegisterSessionAsync();

        //        var response = await client.Roles.SetPermissionAsync(Guid.NewGuid(), new SetRolePermission
        //        {
        //            PermissionId = permissionId,
        //            Active = true
        //        });

        //        Assert.That(response, Is.Not.Null);
        //        Assert.That(response.IsSuccessStatusCode, Is.True);
        //        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        //        serviceBus.VerifyAll();
        //    }
        //}

        //[Test]
        //public async Task Should_be_able_to_get_role_async()
        //{
        //    var activePermissionId = Guid.NewGuid();
        //    var inactivePermissionId = Guid.NewGuid();
        //    var roleQuery = new Mock<IRoleQuery>();

        //    roleQuery.Setup(m => m.Permissions(It.IsAny<Access.DataAccess.Query.Role.Specification>())).Returns(
        //        new List<Access.DataAccess.Query.Role.Permission>
        //        {
        //            new()
        //            {
        //                Id = activePermissionId,
        //                Name = "system://permission-a"
        //            }
        //        });

        //    using (var httpClient = Factory.WithWebHostBuilder(builder =>
        //               {
        //                   builder.ConfigureTestServices(services =>
        //                   {
        //                       services.AddSingleton(roleQuery.Object);
        //                   });
        //               })
        //               .CreateClient())
        //    {
        //        var client = GetClient(httpClient);

        //        var response = await client.Roles.PermissionAvailabilityAsync(Guid.NewGuid(), new Identifiers<Guid>
        //        {
        //            Values = new List<Guid>
        //            {
        //                activePermissionId,
        //                inactivePermissionId
        //            }
        //        });

        //        Assert.That(response, Is.Not.Null);
        //        Assert.That(response.IsSuccessStatusCode, Is.True);
        //        Assert.That(response.Content, Is.Not.Null);

        //        Assert.That(response.Content.Count, Is.EqualTo(2));

        //        var permission = response.Content.Find(item => item.Id == activePermissionId);

        //        Assert.That(permission, Is.Not.Null);
        //        Assert.That(permission.Active, Is.True);

        //        permission = response.Content.Find(item => item.Id == inactivePermissionId);

        //        Assert.That(permission, Is.Not.Null);
        //        Assert.That(permission.Active, Is.False);
        //    }
        //}

        //[Test]
        //public async Task Should_be_able_to_register_role_async()
        //{
        //    var serviceBus = new Mock<IServiceBus>();

        //    using (var httpClient = Factory.WithWebHostBuilder(builder =>
        //           {
        //               builder.ConfigureTestServices(services =>
        //               {
        //                   services.AddSingleton(new Mock<IRoleQuery>().Object);
        //                   services.AddSingleton(serviceBus.Object);
        //               });
        //           }).CreateDefaultClient())
        //    {
        //        var client = await GetClient(httpClient).RegisterSessionAsync();

        //        var response = await client.Roles.RegisterAsync(new RegisterRole
        //        {
        //            Name = "role"
        //        });

        //        Assert.That(response, Is.Not.Null);
        //        Assert.That(response.IsSuccessStatusCode, Is.True);
        //        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

        //        serviceBus.Verify(m => m.Send(It.IsAny<RegisterRole>(), null), Times.Once);
        //    }
        //}
    }
}