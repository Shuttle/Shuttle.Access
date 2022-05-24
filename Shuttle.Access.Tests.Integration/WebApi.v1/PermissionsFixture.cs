using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.Access.Tests.Integration.WebApi.v1
{
    public class PermissionsFixture : WebApiFixture
    {
        [Test]
        public void Should_be_able_to_get_available_permissions()
        {
            var permission = new Access.DataAccess.Query.Permission
            {
                Id = Guid.NewGuid(),
                Name = "integration://available-permission",
                Status = (int)PermissionStatus.Active
            };

            var permissionQuery = new Mock<IPermissionQuery>();

            permissionQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Permission.Specification>())).Returns(new List<Access.DataAccess.Query.Permission> { permission });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IAuthorizationService>().Object);
                           services.AddSingleton(new Mock<IDatabaseContextFactory>().Object);
                           services.AddSingleton(permissionQuery.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).Login();

                var response = client.Permissions.Get().Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                Assert.That(response.Content.Find(item => item.Id == permission.Id), Is.Not.Null);
            }
        }

        [Test]
        public void Should_be_able_to_post_permission()
        {
            const string permission = "integration://available-permission";

            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m => m.Send(It.Is<RegisterPermission>(message => message.Name.Equals(permission)))).Verifiable();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(serviceBus.Object);
                           services.AddSingleton(new Mock<IPermissionQuery>().Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).Login();

                var response = client.Permissions.Post(new RegisterPermission
                {
                    Name = permission
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);

                serviceBus.VerifyAll();
            }
        }

        [Test]
        public void Should_be_able_to_delete_permission()
        {
            var permissionId = Guid.NewGuid();

            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m=>m.Send(It.Is<RemovePermission>(message => message.Id.Equals(permissionId)))).Verifiable();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(serviceBus.Object);
                           services.AddSingleton(new Mock<IPermissionQuery>().Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).Login();

                var response = client.Permissions.Delete(permissionId).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);

                serviceBus.VerifyAll();
            }
        }
    }
}