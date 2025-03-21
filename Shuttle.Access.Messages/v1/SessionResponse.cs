using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1;

public class SessionResponse
{
    public string IdentityName { get; set; } = default!;
    public List<string> Permissions { get; set; } = [];
    public bool RegistrationRequested { get; set; }
    public string Result { get; set; } = default!;
    public Guid Token { get; set; }
    public DateTimeOffset TokenExpiryDate { get; set; }
    public string SessionTokenExchangeUrl { get; set; } = string.Empty;
    public Guid IdentityId { get; set; }
    public DateTimeOffset DateRegistered { get; set; }
}