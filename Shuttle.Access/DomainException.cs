using System;

namespace Shuttle.Access
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
    }
}