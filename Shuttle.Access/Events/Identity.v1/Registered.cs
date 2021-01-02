using System;

namespace Shuttle.Access.Events.Identity.v1
{
    public class Registered
    {
        public string Name { get; set; }
        public byte[] PasswordHash { get; set; }
        public string RegisteredBy { get; set; }
        public DateTime DateRegistered { get; set; }
        public string GeneratedPassword { get; set; }
        public bool Activated { get; set; }
    }
}