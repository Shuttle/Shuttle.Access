using System;

namespace Shuttle.Access.Messages.v1
{
    public class SetIdentityName
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}