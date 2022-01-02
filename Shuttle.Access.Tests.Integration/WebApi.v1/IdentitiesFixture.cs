using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Data;

namespace Shuttle.Access.Tests.Integration.WebApi.v1
{
    public class IdentitiesFixture : WebApiFixture
    {
        [Test]
        public void Should_be_able_to_get_all_identities()
        {
            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IDatabaseContextFactory>().Object);
                           services.AddSingleton(new Mock<IPermissionQuery>().Object);
                       });
                   }).CreateClient())
            {
                var client = GetClient(httpClient);

                var response = client.Identities.Get().Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                //Assert.That(response.Content.Permissions.Find(item => item.Permission == permission), Is.Not.Null);
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
            }
        }
    }
}