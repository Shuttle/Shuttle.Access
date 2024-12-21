using System;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Messages.v1;

public static class MessageExtensions
{
    public static void ApplyInvariants(this RegisterRole message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
    }

    public static void ApplyInvariants(this GetPasswordResetToken message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
    }

    public static void ApplyInvariants(this GetRolePermissions message)
    {
        Guard.AgainstNull(message);
    }

    public static void ApplyInvariants(this SetIdentityRole message)
    {
        Guard.AgainstNull(message);
    }

    public static void ApplyInvariants(this RegisterPasswordReset message)
    {
        Guard.AgainstNull(message);
    }

    public static void ApplyInvariants(this RegisterPasswordExpiry message)
    {
        Guard.AgainstNull(message);
    }

    public static void ApplyInvariants(this ChangePassword message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNull(message.NewPassword, nameof(message.NewPassword));

        if ((!message.Id.HasValue && !message.Token.HasValue) ||
            (message.Id.HasValue && message.Token.HasValue))
        {
            throw new InvalidOperationException(Resources.ChangePasswordException);
        }
    }

    public static void ApplyInvariants(this ResetPassword message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
        Guard.AgainstNullOrEmptyString(message.Password, nameof(message.Password));
    }

    public static void ApplyInvariants(this ActivateIdentity message)
    {
        Guard.AgainstNull(message);

        if (!message.Id.HasValue &&
            string.IsNullOrWhiteSpace(message.Name))
        {
            throw new ArgumentException(Resources.ActivateIdentityException);
        }
    }

    public static void ApplyInvariants(this RegisterIdentity message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNull(message.Name, nameof(message.Name));
    }

    public static void ApplyInvariants(this SetRolePermission message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstEmptyGuid(message.RoleId, nameof(message.RoleId));
        Guard.AgainstEmptyGuid(message.PermissionId, nameof(message.PermissionId));
    }

    public static void ApplyInvariants(this RegisterPermission message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
    }

    public static void ApplyInvariants(this SetIdentityName message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstEmptyGuid(message.Id, nameof(message.Id));
        Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
    }

    public static void ApplyInvariants(this SetRoleName message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstEmptyGuid(message.Id, nameof(message.Id));
        Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
    }

    public static void ApplyInvariants(this SetPermissionName message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstEmptyGuid(message.Id, nameof(message.Id));
        Guard.AgainstNullOrEmptyString(message.Name, nameof(message.Name));
    }

    public static void ApplyInvariants(this SetPermissionStatus message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstEmptyGuid(message.Id, nameof(message.Id));
    }

    public static void ApplyInvariants<T>(this Identifiers<T> message)
    {
        Guard.AgainstNull(message);
        Guard.AgainstNull(message.Values, nameof(message.Values));
    }
}