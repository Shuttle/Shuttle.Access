using System;

namespace Shuttle.Access.Application
{
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message)
        {
        }
    }
}