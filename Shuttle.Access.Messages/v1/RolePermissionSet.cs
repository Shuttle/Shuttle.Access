﻿using System;

namespace Shuttle.Access.Messages.v1;

public class RolePermissionSet
{
    public bool Active { get; set; }
    public Guid PermissionId { get; set; }
    public Guid RoleId { get; set; }
    public long SequenceNumber { get; set; }
}