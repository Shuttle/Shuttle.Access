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
    public class IdentitiesFixture : WebApiFixture
    {
        [Test]
        public void Should_be_able_to_get_all_identities()
        {
            var identityQuery = new Mock<IIdentityQuery>();

            var identity = CreateIdentity();

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

        private static Access.DataAccess.Query.Identity CreateIdentity()
        {
            var now = DateTime.Now;

            return new Access.DataAccess.Query.Identity
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
        }

        [Test]
        public void Should_be_able_to_get_identity_by_value()
        {
            var identityQuery = new Mock<IIdentityQuery>();

            var identity = CreateIdentity();

            identityQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Identity.Specification>())).Returns(new List<Access.DataAccess.Query.Identity>
            {
                identity
            });

            using (var httpClient = Factory.WithWebHostBuilder(builder => { builder.ConfigureTestServices(services => { services.AddSingleton(identityQuery.Object); }); }).CreateClient())
            {
                var client = GetClient(httpClient);

                var response = client.Identities.Get("some-value").Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);
                Assert.That(response.Content.Id, Is.EqualTo(identity.Id));
                Assert.That(response.Content.Name, Is.EqualTo(identity.Name));
                Assert.That(response.Content.DateRegistered, Is.EqualTo(identity.DateRegistered));
                Assert.That(response.Content.DateActivated, Is.EqualTo(identity.DateActivated));
                Assert.That(response.Content.GeneratedPassword, Is.EqualTo(identity.GeneratedPassword));
                Assert.That(response.Content.RegisteredBy, Is.EqualTo(identity.RegisteredBy));
                Assert.That(response.Content.Roles.Find(item => item.Id == identity.Roles[0].Id), Is.Not.Null);
            }
        }

        [Test]
        public void Should_be_able_to_delete_identity()
        {
            var id = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m => m.Send(It.Is<RemoveIdentity>(message => message.Id.Equals(id)))).Verifiable();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient);

                client.Login();

                var getResponse = client.Identities.Delete(id).Result;

                Assert.That(getResponse, Is.Not.Null);
                Assert.That(getResponse.IsSuccessStatusCode, Is.True);

                serviceBus.VerifyAll();
            }
        }
   }
}