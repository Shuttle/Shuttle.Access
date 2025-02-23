using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1;

public class Identity
{
    public DateTimeOffset? DateActivated { get; set; }
    public DateTimeOffset DateRegistered { get; set; }
    public string GeneratedPassword { get; set; } = string.Empty;

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegisteredBy { get; set; } = string.Empty;
    public List<Role> Roles { get; set; } = [];

    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}