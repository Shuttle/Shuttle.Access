using System;

namespace Shuttle.Access.Messages.v1;

public class RegisterPasswordReset
{
    public Guid IdentityId { get; set; }
    public string Token { get; set; } = default!;
}