﻿using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1;

public class ServerConfiguration
{
    public string Version { get; set; } = string.Empty;
    public bool AllowPasswordAuthentication { get; set; }
}