﻿using System;

namespace Shuttle.Access.Messages.v1;

public class RegisterPasswordExpiry
{
    public DateTime ExpiryDate { get; set; }
    public Guid IdentityId { get; set; }
    public bool NeverExpires { get; set; }
}