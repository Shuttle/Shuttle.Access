using Shuttle.Access.DataAccess;
using Shuttle.Access.Messages.v1;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;

namespace Shuttle.Access.Server.Handlers
{
    public class PermissionHandler :
        IMessageHandler<RegisterPermission>,
        IMessageHandler<RemovePermission>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IPermissionQuery _permissionQuery;

        public PermissionHandler(IDatabaseContextFactory databaseContextFactory, IPermissionQuery permissionQuery)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(permissionQuery, nameof(permissionQuery));

            _databaseContextFactory = databaseContextFactory;
            _permissionQuery = permissionQuery;
        }

        public void ProcessMessage(IHandlerContext<RegisterPermission> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                _permissionQuery.Register(message.Permission);
            }

            context.Publish(new PermissionRegistered
            {
                Permission = message.Permission
            });
        }

        public void ProcessMessage(IHandlerContext<RemovePermission> context)
        {
            Guard.AgainstNull(context, nameof(context));

            var message = context.Message;

            using (_databaseContextFactory.Create())
            {
                _permissionQuery.Remove(message.Permission);
            }

            context.Publish(new PermissionRemoved
            {
                Permission = message.Permission
            });
        }
    }
}