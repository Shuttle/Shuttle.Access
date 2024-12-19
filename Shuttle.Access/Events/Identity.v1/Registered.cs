using System;

namespace Shuttle.Access.Events.Identity.v1;

public class Registered
{
    public bool Activated { get; set; }
    public DateTime DateRegistered { get; set; }
    public string GeneratedPassword { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    public string RegisteredBy { get; set; } = string.Empty;
}