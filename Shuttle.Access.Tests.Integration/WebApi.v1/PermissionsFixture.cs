using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Access.WebApi.Models.v1;
using Shuttle.Core.Data;

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
                           services.AddSingleton(new Mock<IDatabaseContextFactory>().Object);
                           services.AddSingleton(new Mock<IPermissionQuery>().Object);
                       });
                   }).CreateClient())
            {
                var client = GetClient(httpClient);

                var response = client.Permissions.GetAnonymous().Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                Assert.That(response.Content.Permissions.Find(item => item.Permission == permission), Is.Not.Null);
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
                Assert.That(response.Content.Find(item => item.Permission == permission), Is.Not.Null);
            }
        }

        [Test]
        public void Should_be_able_to_post_permission()
        {
            const string permission = "integration://available-permission";

            var permissionQuery = new Mock<IPermissionQuery>();
            var permissions = new List<string>();

            permissionQuery.Setup(m => m.Available()).Returns(permissions);
            permissionQuery.Setup(m => m.Register(It.IsAny<string>()))
                .Callback((string registerPermission) => { permissions.Add(registerPermission); });

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

                var getResponse = client.Permissions.Get().Result;

                Assert.That(getResponse, Is.Not.Null);
                Assert.That(getResponse.IsSuccessStatusCode, Is.True);
                Assert.That(getResponse.Content, Is.Not.Null);
                Assert.That(getResponse.Content.Find(item => item.Permission == permission), Is.Null);

                var postResponse = client.Permissions.Post(new PermissionModel
                {
                    Permission = permission
                }).Result;

                Assert.That(postResponse, Is.Not.Null);
                Assert.That(postResponse.IsSuccessStatusCode, Is.True);

                getResponse = client.Permissions.Get().Result;

                Assert.That(getResponse, Is.Not.Null);
                Assert.That(getResponse.IsSuccessStatusCode, Is.True);
                Assert.That(getResponse.Content, Is.Not.Null);
                Assert.That(getResponse.Content.Find(item => item.Permission == permission), Is.Not.Null);
            }
        }

        [Test]
        public void Should_be_able_to_delete_permission()
        {
            const string permission = "integration://available-permission";

            var permissionQuery = new Mock<IPermissionQuery>();
            var permissions = new List<string> { permission };

            permissionQuery.Setup(m => m.Available()).Returns(permissions);
            permissionQuery.Setup(m => m.Remove(It.IsAny<string>()))
                .Callback((string deletePermission) => { permissions.Remove(deletePermission); });

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

                var getResponse = client.Permissions.Get().Result;

                Assert.That(getResponse, Is.Not.Null);
                Assert.That(getResponse.IsSuccessStatusCode, Is.True);
                Assert.That(getResponse.Content, Is.Not.Null);
                Assert.That(getResponse.Content.Find(item => item.Permission == permission), Is.Not.Null);

                var postResponse = client.Permissions.Delete(Uri.EscapeDataString(permission)).Result;

                Assert.That(postResponse, Is.Not.Null);
                Assert.That(postResponse.IsSuccessStatusCode, Is.True);

                getResponse = client.Permissions.Get().Result;

                Assert.That(getResponse, Is.Not.Null);
                Assert.That(getResponse.IsSuccessStatusCode, Is.True);
                Assert.That(getResponse.Content, Is.Not.Null);
                Assert.That(getResponse.Content.Find(item => item.Permission == permission), Is.Null);
            }
        }
    }
}