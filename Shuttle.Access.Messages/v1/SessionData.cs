using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1;

public class SessionData
{
    public DateTimeOffset DateRegistered { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public List<Permission> Permissions { get; set; } = [];
    public string IdentityDescription { get; set; } = string.Empty;
}