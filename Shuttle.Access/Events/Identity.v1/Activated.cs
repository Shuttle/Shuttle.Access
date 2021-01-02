using System;

namespace Shuttle.Access.Events.Identity.v1
{
    public class Activated
    {
        public Guid Id { get; set; }
        public DateTime DateActivated { get; set; }
    }
}