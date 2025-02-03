using System;

namespace Shuttle.Access.Messages.v1;

public class SessionRefreshed
{
    public Guid Token { get; set; }
}