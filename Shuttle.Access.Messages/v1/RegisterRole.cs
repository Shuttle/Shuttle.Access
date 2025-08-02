using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1;

public class RegisterRole
{
    public string Name { get; set; } = default!;
    public List<RegisterPermission> Permissions { get; set; } = [];
    public int WaitCount { get; set; }
}