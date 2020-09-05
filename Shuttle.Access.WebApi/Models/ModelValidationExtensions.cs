using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.WebApi
{
    public static class ModelValidationExtensions
    {
        public static void ApplyInvariants(this AddRoleModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNullOrEmptyString(model.Name, nameof(model.Name));
        }
        
        public static void ApplyInvariants(this RolePermissionStatusModel model)
        {
            Guard.AgainstNull(model, nameof(model));
        }
        
        public static void ApplyInvariants(this SetUserRoleModel model)
        {
            Guard.AgainstNull(model, nameof(model));
        }
        
        public static void ApplyInvariants(this RegisterPasswordResetModel model)
        {
            Guard.AgainstNull(model, nameof(model));
        }
        
        public static void ApplyInvariants(this RegisterPasswordExpiryModel model)
        {
            Guard.AgainstNull(model, nameof(model));
        }
        
        public static void ApplyInvariants(this SetPasswordModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNull(model.NewPassword, nameof(model.NewPassword));

            if (string.IsNullOrWhiteSpace(model.OldPassword) && string.IsNullOrWhiteSpace(model.Token))
            {
                throw new InvalidOperationException(Access.Resources.SetPasswordException);
            }
        }
        
        public static void ApplyInvariants(this UserRoleStatusModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNull(model.RoleIds, nameof(model.RoleIds));
        }
        
        public static void ApplyInvariants(this RegisterUserModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNull(model.Username, nameof(model.Username));
        }

        public static void ApplyInvariants(this SetRolePermissionModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNull(model.Permission, nameof(model.Permission));
        }
    }
}