using System;

namespace Shuttle.Access.Messages.v1;

public class RegisterIdentity
{
    public bool Activated { get; set; }
    public string GeneratedPassword { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public byte[] PasswordHash { get; set; } = [];
    public string RegisteredBy { get; set; } = string.Empty;
    public string System { get; set; } = string.Empty;
}