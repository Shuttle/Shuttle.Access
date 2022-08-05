using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Testing;
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
    public class IdentitiesFixture : WebApiFixture
    {
        [Test]
        public void Should_be_able_to_get_all_identities()
        {
            var identityQuery = new Mock<IIdentityQuery>();

            var identity = CreateIdentity();

            identityQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Identity.Specification>())).Returns(
                new List<Access.DataAccess.Query.Identity>
                {
                    identity
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(identityQuery.Object);
                       });
                   }).CreateClient())
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
            var now = DateTime.UtcNow;

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

            identityQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Identity.Specification>())).Returns(
                new List<Access.DataAccess.Query.Identity>
                {
                    identity
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(identityQuery.Object);
                       });
                   }).CreateClient())
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

            serviceBus.Setup(m => m.Send(It.Is<RemoveIdentity>(message => message.Id.Equals(id)), null)).Verifiable();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).RegisterSession();

                var response = client.Identities.Delete(id).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);

                serviceBus.VerifyAll();
            }
        }

        [Test]
        public void Should_be_able_to_set_identity_role_status()
        {
            var roleId = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m => m.Send(It.Is<SetIdentityRole>(message => message.RoleId.Equals(roleId)), null))
                .Verifiable();

            var mediator = new Mock<IMediator>();

            mediator.Setup(m => m.Send(It.IsAny<RequestMessage<SetIdentityRole>>(), CancellationToken.None))
                .Verifiable();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).RegisterSession();

                var response = client.Identities.SetRole(Guid.NewGuid(), roleId, new SetIdentityRole
                {
                    Active = true
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

                serviceBus.VerifyAll();
                mediator.VerifyAll();
            }
        }

        [Test]
        public void Should_not_be_able_to_set_identity_role_status_when_mediator_call_fails()
        {
            var roleId = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();
            var mediator = new Mock<IMediator>();

            mediator.Setup(m => m.Send(It.IsAny<RequestMessage<SetIdentityRole>>(), CancellationToken.None))
                .Callback<object, CancellationToken>((message, _) =>
                {
                    ((RequestMessage<SetIdentityRole>)message).Failed("reason");
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).RegisterSession();

                var response = client.Identities.SetRole(Guid.NewGuid(), roleId, new SetIdentityRole
                {
                    IdentityId = Guid.NewGuid()
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.False);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                serviceBus.Verify(m => m.Send(It.IsAny<object>(), null), Times.Never);
            }
        }

        [Test]
        public void Should_not_be_able_to_change_password_when_no_session_token_is_provided()
        {
            var token = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();
            var mediator = new Mock<IMediator>();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient);

                httpClient.DefaultRequestHeaders.Remove("Authorization");

                client.RegisterSession();

                var response = client.Identities.ChangePassword(new ChangePassword
                {
                    NewPassword = "new-password",
                    Token = token
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.False);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                mediator.Verify(m => m.Send(It.IsAny<ChangePassword>(), CancellationToken.None), Times.Never);
                serviceBus.Verify(m => m.Send(It.IsAny<object>(), null), Times.Never);
            }
        }

        [Test]
        public void Should_not_be_able_to_change_password_when_mediator_call_fails()
        {
            var token = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();
            var mediator = new Mock<IMediator>();

            mediator.Setup(m => m.Send(It.IsAny<RequestMessage<ChangePassword>>(), CancellationToken.None))
                .Callback<object, CancellationToken>((message, _) =>
                {
                    ((RequestMessage<ChangePassword>)message).Failed("reason");
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient);

                client.RegisterSession();

                var response = client.Identities.ChangePassword(new ChangePassword
                {
                    NewPassword = "new-password",
                    Token = token
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.False);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                mediator.Verify(m => m.Send(It.IsAny<ChangePassword>(), CancellationToken.None), Times.Never);
                serviceBus.Verify(m => m.Send(It.IsAny<object>(), null), Times.Never);
            }
        }

        [Test]
        public void Should_be_able_to_change_password()
        {
            var token = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m => m.Send(It.Is<ChangePassword>(message => message.Token.Equals(token)), null)).Verifiable();

            var mediator = new Mock<IMediator>();

            mediator.Setup(m => m.Send(It.IsAny<RequestMessage<ChangePassword>>(), CancellationToken.None))
                .Verifiable();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient);

                client.RegisterSession();

                var response = client.Identities.ChangePassword(new ChangePassword
                {
                    NewPassword = "new-password",
                    Token = token
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

                mediator.Verify(m => m.Send(It.IsAny<ChangePassword>(), CancellationToken.None), Times.Never);
                serviceBus.Verify(m => m.Send(It.IsAny<object>(), null), Times.Never);
            }
        }

        [Test]
        public void Should_not_be_able_to_reset_password_when_no_session_token_is_provided()
        {
            var token = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();
            var mediator = new Mock<IMediator>();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient);

                httpClient.DefaultRequestHeaders.Remove("Authorization");

                client.RegisterSession();

                var response = client.Identities.ResetPassword(new ResetPassword
                {
                    Name = "identity",
                    Password = "password",
                    PasswordResetToken = token
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.False);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));

                mediator.Verify(m => m.Send(It.IsAny<ResetPassword>(), CancellationToken.None), Times.Never);
                serviceBus.Verify(m => m.Send(It.IsAny<object>(), null), Times.Never);
            }
        }

        [Test]
        public void Should_not_be_able_to_reset_password_when_mediator_call_fails()
        {
            var token = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();
            var mediator = new Mock<IMediator>();

            mediator.Setup(m => m.Send(It.IsAny<RequestMessage<ResetPassword>>(), CancellationToken.None))
                .Callback<object, CancellationToken>((message, _) =>
                {
                    ((RequestMessage<ResetPassword>)message).Failed("reason");
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).RegisterSession();

                var response = client.Identities.ResetPassword(new ResetPassword
                {
                    Name = "identity",
                    Password = "password",
                    PasswordResetToken = token
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.False);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                mediator.Verify(m => m.Send(It.IsAny<ResetPassword>(), CancellationToken.None), Times.Never);
                serviceBus.Verify(m => m.Send(It.IsAny<object>(), null), Times.Never);
            }
        }

        [Test]
        public void Should_be_able_to_reset_password()
        {
            var token = Guid.NewGuid();
            var serviceBus = new Mock<IServiceBus>();

            serviceBus.Setup(m => m.Send(It.Is<ResetPassword>(message => message.PasswordResetToken.Equals(token)), null))
                .Verifiable();

            var mediator = new Mock<IMediator>();

            mediator.Setup(m => m.Send(It.IsAny<RequestMessage<ResetPassword>>(), CancellationToken.None)).Verifiable();

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient);

                client.RegisterSession();

                var response = client.Identities.ResetPassword(new ResetPassword
                {
                    Name = "identity",
                    Password = "password",
                    PasswordResetToken = token
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                mediator.Verify(m => m.Send(It.IsAny<ResetPassword>(), CancellationToken.None), Times.Never);
                serviceBus.Verify(m => m.Send(It.IsAny<object>(), null), Times.Never);
            }
        }

        [Test]
        public void Should_be_able_to_get_role_status()
        {
            var activeRoleId = Guid.NewGuid();
            var inactiveRoleId = Guid.NewGuid();
            var identityQuery = new Mock<IIdentityQuery>();

            identityQuery.Setup(m => m.RoleIds(It.IsAny<Access.DataAccess.Query.Identity.Specification>())).Returns(
                new List<Guid>
                {
                    activeRoleId
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                       {
                           builder.ConfigureTestServices(services =>
                           {
                               services.AddSingleton(identityQuery.Object);
                           });
                       })
                       .CreateClient())
            {
                var client = GetClient(httpClient);

                var response = client.Identities.RoleAvailability(Guid.NewGuid(), new Identifiers<Guid>
                {
                    Values = new List<Guid>
                    {
                        activeRoleId,
                        inactiveRoleId
                    }
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content, Is.Not.Null);

                Assert.That(response.Content.Count, Is.EqualTo(2));

                var identityRoleStatus = response.Content.Find(item => item.Id == activeRoleId);

                Assert.That(identityRoleStatus, Is.Not.Null);
                Assert.That(identityRoleStatus.Active, Is.True);

                identityRoleStatus = response.Content.Find(item => item.Id == inactiveRoleId);

                Assert.That(identityRoleStatus, Is.Not.Null);
                Assert.That(identityRoleStatus.Active, Is.False);
            }
        }

        [Test]
        public void Should_not_be_able_to_activate_unknown_identity()
        {
            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                       });
                   }).CreateClient())
            {
                var client = GetClient(httpClient).RegisterSession();

                var response = client.Identities.Activate(new ActivateIdentity
                {
                    Name = "unknown"
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.False);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            }
        }

        [Test]
        public void Should_be_able_to_activate_identity()
        {
            var serviceBus = new Mock<IServiceBus>();
            var identityQuery = new Mock<IIdentityQuery>();

            var identity = CreateIdentity();

            identityQuery.Setup(m => m.Search(It.IsAny<Access.DataAccess.Query.Identity.Specification>())).Returns(
                new List<Access.DataAccess.Query.Identity>
                {
                    identity
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder
                           .ConfigureTestServices(services =>
                       {
                           services.AddSingleton(identityQuery.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateClient())
            {
                var client = GetClient(httpClient).RegisterSession();

                var response = client.Identities.Activate(new ActivateIdentity
                {
                    Name = "known"
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

                serviceBus.Verify(m => m.Send(It.IsAny<ActivateIdentity>(), null), Times.Once);
            }
        }

        [Test]
        public void Should_be_able_to_get_password_reset_token()
        {
            var token = Guid.NewGuid();
            var mediator = new Mock<IMediator>();

            mediator.Setup(m =>
                    m.Send(It.IsAny<RequestResponseMessage<GetPasswordResetToken, Guid>>(), CancellationToken.None))
                .Callback<object, CancellationToken>((message, _) =>
                {
                    ((RequestResponseMessage<GetPasswordResetToken, Guid>)message).WithResponse(token);
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).RegisterSession();

                var response = client.Identities.GetPasswordResetToken("identity").Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(response.Content, Is.EqualTo(token));

                mediator.Verify(
                    m => m.Send(It.IsAny<RequestResponseMessage<GetPasswordResetToken, Guid>>(),
                        CancellationToken.None), Times.Once);
            }
        }

        [Test]
        public void Should_not_be_able_to_get_password_reset_token_when_mediator_call_fails()
        {
            var mediator = new Mock<IMediator>();

            mediator.Setup(m =>
                    m.Send(It.IsAny<RequestResponseMessage<GetPasswordResetToken, Guid>>(), CancellationToken.None))
                .Callback<object, CancellationToken>((message, _) =>
                {
                    ((RequestResponseMessage<GetPasswordResetToken, Guid>)message).Failed("reason");
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).RegisterSession();

                var response = client.Identities.GetPasswordResetToken("identity").Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.False);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                mediator.Verify(
                    m => m.Send(It.IsAny<RequestResponseMessage<GetPasswordResetToken, Guid>>(),
                        CancellationToken.None), Times.Once);
            }
        }

        [Test]
        public void Should_be_able_to_register_identity()
        {
            var serviceBus = new Mock<IServiceBus>();
            var mediator = new Mock<IMediator>();

            mediator.Setup(m => m.Send(It.IsAny<IdentityRegistrationRequested>(), CancellationToken.None))
                .Callback<object, CancellationToken>((message, _) =>
                {
                    ((IdentityRegistrationRequested)message).Allowed("test", true);
                });

            mediator.Setup(m => m.Send(It.IsAny<GeneratePassword>(), CancellationToken.None))
                .Callback<object, CancellationToken>((message, _) =>
                {
                    ((GeneratePassword)message).GeneratedPassword = "generated-password";
                });

            mediator.Setup(m => m.Send(It.IsAny<GenerateHash>(), CancellationToken.None))
                .Callback<object, CancellationToken>((message, _) =>
                {
                    ((GenerateHash)message).Hash = new byte[] { 0, 1, 2, 3, 4 };
                });

            using (var httpClient = Factory.WithWebHostBuilder(builder =>
                   {
                       builder.ConfigureTestServices(services =>
                       {
                           services.AddSingleton(new Mock<IIdentityQuery>().Object);
                           services.AddSingleton(mediator.Object);
                           services.AddSingleton(serviceBus.Object);
                       });
                   }).CreateDefaultClient())
            {
                var client = GetClient(httpClient).RegisterSession();

                var response = client.Identities.Register(new RegisterIdentity
                {
                    Name = "identity"
                }).Result;

                Assert.That(response, Is.Not.Null);
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Accepted));

                mediator.Verify(m => m.Send(It.IsAny<IdentityRegistrationRequested>(), CancellationToken.None),
                    Times.Once);
                mediator.Verify(m => m.Send(It.IsAny<GeneratePassword>(), CancellationToken.None), Times.Once);
                mediator.Verify(m => m.Send(It.IsAny<GenerateHash>(), CancellationToken.None), Times.Once);
                serviceBus.Verify(m => m.Send(It.IsAny<RegisterIdentity>(), null), Times.Once);
            }
        }
    }
}