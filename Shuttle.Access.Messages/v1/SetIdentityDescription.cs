using System;

namespace Shuttle.Access.Messages.v1;

public class SetIdentityDescription
{
    public Guid Id { get; set; }
    public string Description{ get; set; } = string.Empty;
}