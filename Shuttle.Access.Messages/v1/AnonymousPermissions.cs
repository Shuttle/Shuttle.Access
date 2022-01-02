using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1
{
    public class AnonymousPermissions
    {
        public bool IsIdentityRequired { get; set; }
        public List<string> Permissions { get; set; }
    }
}