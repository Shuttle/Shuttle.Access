using System;

namespace Shuttle.Access
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException(string message) : base(message)
        {
        }

        public static EntityNotFoundException For(string name, Guid id)
        {
            return new EntityNotFoundException($"Could not find an entity '{name}' with id '{id}'.");
        }
    }
}