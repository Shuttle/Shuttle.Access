namespace Shuttle.Access
{
    public static class Permissions
    {
        public static class View
        {
            public const string Role = "access://role/view";
            public const string Identity = "access://identity/view";
            public const string Permission = "access://permissions/view";
        }

        public static class Register
        {
            public const string IdentityRequired = "access://identity/required";
            public const string Role = "access://roles/register";
            public const string Identity = "access://identity/register";
            public const string Permission = "access://permission/register";
        }

        public static class Remove
        {
            public const string Role = "access://role/remove";
            public const string Identity = "access://identity/remove";
            public const string Permission = "access://permission/remove";
        }
    }
}