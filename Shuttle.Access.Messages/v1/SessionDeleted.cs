using System;

namespace Shuttle.Access.Messages.v1;

public class SessionDeleted
{
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
}