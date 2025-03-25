using System;

namespace Shuttle.Access.Messages.v1;

public class SessionRefreshed
{
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
}