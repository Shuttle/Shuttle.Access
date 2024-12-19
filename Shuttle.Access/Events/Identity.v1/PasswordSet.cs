using System;

namespace Shuttle.Access.Events.Identity.v1;

public class PasswordSet
{
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
}