using System;

namespace Shuttle.Access.Messages.v1
{
    public class IdentityRegistered
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
	    public string RegisteredBy { get; set; }
        public string GeneratedPassword { get; set; }
        public bool Activated { get; set; }
        public string System { get; set; }
    }
}
