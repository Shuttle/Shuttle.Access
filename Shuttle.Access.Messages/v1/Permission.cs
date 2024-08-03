using System;
using System.Collections.Generic;

namespace Shuttle.Access.Messages.v1
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }

        public class Specification
        {
            public List<Guid> Ids { get; set; }
        }
    }
}