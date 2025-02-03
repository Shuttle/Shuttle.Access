﻿using System;

namespace Shuttle.Access.Messages.v1;

public class PermissionNameSet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long SequenceNumber { get; set; }
}