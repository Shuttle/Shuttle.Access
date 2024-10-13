namespace Shuttle.Access
{
    public static class AccessPermissions
    {
        public static class Identities
        {
            public const string Activate = "access://identities/activate";
            public const string Manage = "access://identities/manage";
            public const string Register = "access://identities/register";
            public const string Remove = "access://identities/remove";
            public const string View = "access://identities/view";
        }

        public static class Roles
        {
            public const string View = "access://roles/view";
            public const string Register = "access://roles/register";
            public const string Remove = "access://roles/remove";
        }

        public static class Permissions
        {
            public const string Manage = "access://permissions/manage";
            public const string Register = "access://permissions/register";
            public const string View = "access://permissions/view";
        }

        public static class Sessions
        {
            public const string View = "access://sessions/view";
            public const string Register = "access://sessions/register";
        }
    }
}