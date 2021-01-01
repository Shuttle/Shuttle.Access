using System;

namespace Shuttle.Access.Events.User.v1
{
    public class Activated
    {
        public Guid Id { get; set; }
        public DateTime DateActivated { get; set; }
    }
}