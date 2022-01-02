using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Messages.v1
{
    public static class MessageExtensions
    {
        public static void ApplyInvariants(this AddRole message)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
        }
        
        public static void ApplyInvariants(this GetPasswordResetToken message)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
        }
        
        public static void ApplyInvariants(this GetRolePermissions message)
        {
            Guard.AgainstNull(message, nameof(message));
        }
        
        public static void ApplyInvariants(this SetIdentityRoleStatus message)
        {
            Guard.AgainstNull(message, nameof(message));
        }
        
        public static void ApplyInvariants(this RegisterPasswordReset message)
        {
            Guard.AgainstNull(message, nameof(message));
        }
        
        public static void ApplyInvariants(this RegisterPasswordExpiry message)
        {
            Guard.AgainstNull(message, nameof(message));
        }
        
        public static void ApplyInvariants(this ChangePassword message)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNull(message.NewPassword, nameof(message.NewPassword));

            if (string.IsNullOrWhiteSpace(message.OldPassword) && string.IsNullOrWhiteSpace(message.Token))
            {
                throw new InvalidOperationException(Resources.SetPasswordException);
            }
        }
        
        public static void ApplyInvariants(this ResetPassword message)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
            Guard.AgainstNullOrEmptyString(message.Password, nameof(message.Password));
        }
        
        public static void ApplyInvariants(this GetIdentityRoleStatus message)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNull(message.RoleIds, nameof(message.RoleIds));
        }
        
        public static void ApplyInvariants(this ActivateIdentity message)
        {
            Guard.AgainstNull(message, nameof(message));

            if (!message.Id.HasValue &&
                string.IsNullOrWhiteSpace(message.Name))
            {
                throw new ArgumentException(Resources.ActivateIdentityException);
            }
        }
        
        public static void ApplyInvariants(this RegisterIdentity message)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNull(message.Name, nameof(message.Name));
        }

        public static void ApplyInvariants(this SetRolePermissionStatus message)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNullOrEmptyString(message.Permission, nameof(message.Permission));
        }

        public static void ApplyInvariants(this RegisterPermission message)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNullOrEmptyString(message.Permission, nameof(message.Permission));
        }

        public static void ApplyInvariants(this GetRolePermissionStatus message)
        {
            Guard.AgainstNull(message, nameof(message));
            Guard.AgainstNull(message.Permissions, nameof(message.Permissions));
        }
    }
}