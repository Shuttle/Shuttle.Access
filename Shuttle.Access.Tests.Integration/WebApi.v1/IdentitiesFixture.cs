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
            var now = DateTime.Now;
            var identityQuery = new Mock<IIdentityQuery>();

            var identity = new Access.DataAccess.Query.Identity
            {
                Id = Guid.NewGuid(),
                Name = "name",
                DateRegistered = now,
                DateActivated = now,
                GeneratedPassword = "generated-password",
                RegisteredBy = "system",
                Roles = new List<Access.DataAccess.Query.Identity.Role>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = "role"
                    }
                }
            };

            identityQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Identity.Specification>())).Returns(new List<Access.DataAccess.Query.Identity>
            {
                identity
            });

            using (var httpClient = Factory.WithWebHostBuilder(builder => { builder.ConfigureTestServices(services => { services.AddSingleton(identityQuery.Object); }); }).CreateClient())
            {
                var client = GetClient(httpClient);

                var response = client.Identities.Get().Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);

                Assert.That(response.Content.Count, Is.EqualTo(1));

                var responseIdentity = response.Content[0];

                Assert.That(responseIdentity.Id, Is.EqualTo(identity.Id));
                Assert.That(responseIdentity.Name, Is.EqualTo(identity.Name));
                Assert.That(responseIdentity.DateRegistered, Is.EqualTo(identity.DateRegistered));
                Assert.That(responseIdentity.DateActivated, Is.EqualTo(identity.DateActivated));
                Assert.That(responseIdentity.GeneratedPassword, Is.EqualTo(identity.GeneratedPassword));
                Assert.That(responseIdentity.RegisteredBy, Is.EqualTo(identity.RegisteredBy));
                Assert.That(responseIdentity.Roles.Find(item => item.Id == identity.Roles[0].Id), Is.Not.Null);
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