using System;

namespace Shuttle.Access.Events.User.v1
{
    public class Registered
    {
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public string RegisteredBy { get; set; }
        public DateTime DateRegistered { get; set; }
        public string GeneratedPassword { get; set; }
    }
}