using System;

namespace Shuttle.Access.Messages.v1;

public class SessionRefreshed
{
    public string IdentityName { get; set; } = string.Empty;
}