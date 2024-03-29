﻿using System;

namespace Shuttle.Access
{
    public class AccessOptions
    {
        public const string SectionName = "Shuttle:Access";

        public string ConnectionStringName { get; set; } = "Access";
        public TimeSpan SessionDuration { get; set; } = TimeSpan.FromHours(8);
    }
}