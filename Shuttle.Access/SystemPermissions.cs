namespace Shuttle.Access
{
    public static class SystemPermissions
    {
        public static class View
        {
            public const string Roles = "access://roles/view";
            public const string Users = "access://users/view";
            public const string Permissions = "access://permissions/view";
        }

        public static class Register
        {
            public const string UserRequired = "access://user/required";
            public const string Roles = "access://roles/register";
            public const string Users = "access://users/register";
            public const string Permissions = "access://permission/register";
        }

        public static class Remove
        {
            public const string Roles = "access://roles/remove";
            public const string Users = "access://users/remove";
            public const string Permissions = "access://permission/remove";
        }
    }
}