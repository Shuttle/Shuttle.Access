using System;

namespace Shuttle.Access.Messages.v1;

public class IdentityRegistered
{
    public bool Activated { get; set; }
    public string GeneratedPassword { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegisteredBy { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
    public string System { get; set; } = string.Empty;
}