using System;

namespace Shuttle.Access.Messages.v1
{
    public class IdentityActivated
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateActivated { get; set; }
    }
}