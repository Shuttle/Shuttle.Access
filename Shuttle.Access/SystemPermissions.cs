namespace Shuttle.Access
{
    public static class SystemPermissions
    {
        public static class Manage
        {
            public const string Roles = "access://roles/manage";
            public const string Users = "access://users/manage";
        }

        public static class View
        {
            public const string Roles = "access://roles/view";
            public const string Users = "access://users/view";
        }

        public static class Register
        {
            public const string UserRequired = "access://user/required";
        }
    }
}