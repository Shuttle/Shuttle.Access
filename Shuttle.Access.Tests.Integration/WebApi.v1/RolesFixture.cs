using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Mediator;
using Shuttle.Esb;

namespace Shuttle.Access.Tests.Integration.WebApi.v1
{
    public class RolesFixture : WebApiFixture
    {
        [Test]
        public void Should_be_able_to_get_all_roles()
        {
            var roleQuery = new Mock<IRoleQuery>();

            var role = CreateRole();

            roleQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Role.Specification>())).Returns(
                new List<Access.DataAccess.Query.Role>
                {
                    role
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(roleQuery.Object);
                       });
                   }).CreateClient())
            {
                var client = GetClient(httpClient);

                var response = client.Roles.Get().Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);

                Assert.That(response.Content.Count, Is.EqualTo(1));

                var responseRole = response.Content[0];

                Assert.That(responseRole.Id, Is.EqualTo(role.Id));
                Assert.That(responseRole.RoleName, Is.EqualTo(role.RoleName));
                Assert.That(responseRole.Permissions.Find(item => item == role.Permissions[0]), Is.Not.Null);
                Assert.That(responseRole.Permissions.Find(item => item == role.Permissions[1]), Is.Not.Null);
            }
        }

        private static Access.DataAccess.Query.Role CreateRole()
        {
            return new Access.DataAccess.Query.Role
            {
                Id = Guid.NewGuid(),
                RoleName = "name",
                Permissions = new List<string>
                {
                    "system://permission-a",
                    "system://permission-b",
                }
            };
        }

        [Test]
        public void Should_be_able_to_get_role_by_value()
        {
            var roleQuery = new Mock<IRoleQuery>();

            var role = CreateRole();

            roleQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Role.Specification>())).Returns(
                new List<Access.DataAccess.Query.Role>
                {
                    role
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(roleQuery.Object);
                       });
                   }).CreateClient())
            {
                var client = GetClient(httpClient);

                var response = client.Roles.Get("some-value").Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                Assert.That(response.Content.Id, Is.EqualTo(role.Id));
                Assert.That(response.Content.RoleName, Is.EqualTo(role.RoleName));
                Assert.That(response.Content.Permissions.Find(item => item == role.Permissions[0]), Is.Not.Null);
                Assert.That(response.Content.Permissions.Find(item => item == role.Permissions[1]), Is.Not.Null);
            }
        }

        [Test]
        public void Should_be_able_to_delete_role()
        {
            var id = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m => m.Send(It.Is<RemoveRole>(message => message.Id.Equals(id)))).Verifiable();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IRoleQuery>().Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).Login();

                var response = client.Roles.Delete(id).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);

                serviceBus.VerifyAll();
            }
        }

        [Test]
        public void Should_be_able_to_set_role_role_status()
        {
            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m => m.Send(It.Is<SetRolePermissionStatus>(message => message.Permission.Equals("system://permission-a"))))
                .Verifiable();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IRoleQuery>().Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).Login();

                var response = client.Roles.SetPermissionStatus(Guid.NewGuid(), new SetRolePermissionStatus
                {
                    Permission = "system://permission-a",
                    Active = true
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

                serviceBus.VerifyAll();
            }
        }

        [Test]
        public void Should_not_be_able_to_set_role_role_status_when_mediator_call_fails()
        {
            var serviceBus = new Mock<IServiceBus>();
            var mediator = new Mock<IMediator>();

            mediator.Setup(m => m.Send(It.IsAny<RequestMessage<SetRolePermissionStatus>>(), CancellationToken.None))
                .Callback<object, CancellationToken>((message, _) =>
                {
                    ((RequestMessage<SetRolePermissionStatus>)message).Failed("reason");
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IRoleQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).Login();

                var response = client.Roles.SetPermissionStatus(Guid.NewGuid(), new SetRolePermissionStatus
                {
                    RoleId = Guid.NewGuid()
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.False);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                serviceBus.Verify(m => m.Send(It.IsAny<object>()), Times.Never);
            }
        }

        [Test]
        public void Should_be_able_to_get_role_status()
        {
            var activePermission = "system://permission-a";
            var inactivePermission = "system://permission-b";
            var roleQuery = new Mock<IRoleQuery>();

            roleQuery.Setup(m => m.Permissions(It.IsAny<Access.DataAccess.Query.Role.Specification>())).Returns(
                new List<Access.DataAccess.Query.Role.RolePermission>
                {
                    new()
                    {
                        Permission = activePermission
                    }
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                       {
                           builder.ConfigureTestServices(services =>
                           {
                               services.AddSingleton(roleQuery.Object);
                           });
                       })
                       .CreateClient())
            {
                var client = GetClient(httpClient);

                var response = client.Roles.GetPermissionStatus(Guid.NewGuid(), new Identifiers<string>
                {
                    Values = new List<string>
                    {
                        activePermission,
                        inactivePermission
                    }
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);

                Assert.That(response.Content.Count, Is.EqualTo(2));

                var rolePermissionStatus = response.Content.Find(item => item.Permission == activePermission);

                Assert.That(rolePermissionStatus, Is.Not.Null);
                Assert.That(rolePermissionStatus.Active, Is.True);

                rolePermissionStatus = response.Content.Find(item => item.Permission == inactivePermission);

                Assert.That(rolePermissionStatus, Is.Not.Null);
                Assert.That(rolePermissionStatus.Active, Is.False);
            }
        }

        [Test]
        public void Should_be_able_to_register_role()
        {
            var serviceBus = new Mock<IServiceBus>();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IRoleQuery>().Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).Login();

                var response = client.Roles.Register(new RegisterRole
                {
                    Name = "role"
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

                serviceBus.Verify(m => m.Send(It.IsAny<RegisterRole>()), Times.Once);
            }
        }
    }
}