﻿using System;

namespace Shuttle.Access.Messages.v1;

public class PermissionRegistered
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public long SequenceNumber { get; set; }
}