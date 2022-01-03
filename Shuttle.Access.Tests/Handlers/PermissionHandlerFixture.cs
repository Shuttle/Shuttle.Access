using Moq;
using NUnit.Framework;
using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Access.Server.Handlers;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.Access.Tests.Handlers
{
    [TestFixture]
    public class PermissionHandlerFixture
    {
        const string Permission = "integration://anonymous-permission";

        [Test]
        public void Should_be_able_to_register_permission()
        {
            var permissionQuery = new Mock<IPermissionQuery>();

            permissionQuery.Setup(m => m.Register(Permission)).Verifiable();

            var handler = new PermissionHandler(new Mock<IDatabaseContextFactory>().Object, permissionQuery.Object);

            var context = new Mock<IHandlerContext<RegisterPermission>>();

            context.Setup(m => m.Publish(It.Is<PermissionRegistered>(message => message.Permission.Equals(Permission)))).Verifiable();
            context.Setup(m => m.Message).Returns(new RegisterPermission { Permission = Permission });

            handler.ProcessMessage(context.Object);

            permissionQuery.VerifyAll();
            context.VerifyAll();
        }

        [Test]
        public void Should_be_able_to_remove_permission()
        {
            var permissionQuery = new Mock<IPermissionQuery>();

            permissionQuery.Setup(m => m.Remove(Permission)).Verifiable();

            var handler = new PermissionHandler(new Mock<IDatabaseContextFactory>().Object, permissionQuery.Object);

            var context = new Mock<IHandlerContext<RemovePermission>>();

            context.Setup(m => m.Publish(It.Is<PermissionRemoved>(message => message.Permission.Equals(Permission)))).Verifiable();
            context.Setup(m => m.Message).Returns(new RemovePermission { Permission = Permission });

            handler.ProcessMessage(context.Object);

            permissionQuery.VerifyAll();
            context.VerifyAll();
        }
    }
}