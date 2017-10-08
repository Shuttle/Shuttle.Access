using System;

namespace Shuttle.Access.Messages.v1
{
    public class SetUserRoleCommand
    {
        public Guid UserId { get; set; }
        public string RoleName { get; set; }
        public bool Active { get; set; }
    }
}