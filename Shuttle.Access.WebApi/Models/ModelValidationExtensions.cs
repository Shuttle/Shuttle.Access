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
        
        public static void ApplyInvariants(this GetPasswordResetTokenModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNullOrEmptyString(model.Name, nameof(model.Name));
        }
        
        public static void ApplyInvariants(this RolePermissionStatusModel model)
        {
            Guard.AgainstNull(model, nameof(model));
        }
        
        public static void ApplyInvariants(this SetIdentityRoleModel model)
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
        
        public static void ApplyInvariants(this ResetPasswordModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNullOrEmptyString(model.Name, nameof(model.Name));
            Guard.AgainstNullOrEmptyString(model.Password, nameof(model.Password));
        }
        
        public static void ApplyInvariants(this IdentityRoleStatusModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNull(model.RoleIds, nameof(model.RoleIds));
        }
        
        public static void ApplyInvariants(this IdentityActivateModel model)
        {
            Guard.AgainstNull(model, nameof(model));

            if (!model.Id.HasValue &&
                string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentException(Resources.IdentityActivateModelException);
            }
        }
        
        public static void ApplyInvariants(this RegisterIdentityModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNull(model.Name, nameof(model.Name));
        }

        public static void ApplyInvariants(this SetRolePermissionModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNull(model.Permission, nameof(model.Permission));
        }

        public static void ApplyInvariants(this PermissionModel model)
        {
            Guard.AgainstNull(model, nameof(model));
            Guard.AgainstNullOrEmptyString(model.Permission, nameof(model.Permission));
        }
    }
}