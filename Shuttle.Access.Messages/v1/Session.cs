﻿using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1;

public class Session
{
    public DateTimeOffset DateRegistered { get; set; }
    public DateTimeOffset ExpiryDate { get; set; }
    public Guid IdentityId { get; set; }
    public string IdentityName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = [];

    public class Specification
    {
        public Guid? Token { get; set; }
        public Guid? IdentityId { get; set; }
        public string IdentityName { get; set; } = string.Empty;
        public string IdentityNameMatch { get; set; } = string.Empty;
        public bool ShouldIncludePermissions { get; set; }
    }
}