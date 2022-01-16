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
        public void Should_be_able_to_get_anonymous_permissions()
        {
            const string permission = "integration://anonymous-permission";

            var authorizationService = new Mock<IAuthorizationService>();
            var anonymousPermissions = authorizationService.As<IAnonymousPermissions>();

            anonymousPermissions.Setup(m => m.AnonymousPermissions()).Returns(new List<string> { permission });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(authorizationService.Object);
                           services.AddSingleton(new Mock<IPermissionQuery>().Object);
                       });
                   }).CreateClient())
            {
                var client = GetClient(httpClient);

                var response = client.Permissions.GetAnonymous().Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                Assert.That(response.Content.Permissions.Find(item => item == permission), Is.Not.Null);
            }
        }

        [Test]
        public void Should_be_able_to_get_available_permissions()
        {
            const string permission = "integration://available-permission";

            var permissionQuery = new Mock<IPermissionQuery>();

            permissionQuery.Setup(m => m.Available()).Returns(new List<string> { permission });

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
                var client = GetClient(httpClient);

                client.Login();

                var response = client.Permissions.Get().Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                Assert.That(response.Content.Find(item => item == permission), Is.Not.Null);
            }
        }

        [Test]
        public void Should_be_able_to_post_permission()
        {
            const string permission = "integration://available-permission";

            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m => m.Send(It.Is<RegisterPermission>(message => message.Permission.Equals(permission)))).Verifiable();

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
                    Permission = permission
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);

                serviceBus.VerifyAll();
            }
        }

        [Test]
        public void Should_be_able_to_delete_permission()
        {
            const string permission = "integration://available-permission";

            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m=>m.Send(It.Is<RemovePermission>(message => message.Permission.Equals(permission)))).Verifiable();

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

                var response = client.Permissions.Delete(Uri.EscapeDataString(permission)).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);

                serviceBus.VerifyAll();
            }
        }
    }
}