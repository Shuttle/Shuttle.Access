using System;

namespace Shuttle.Access.Messages.v1;

public class ChangePassword
{
    public Guid? Id { get; set; }
    public string NewPassword { get; set; } = string.Empty;
    public Guid? Token { get; set; }
}