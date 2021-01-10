using System;

namespace Shuttle.Access.Api
{
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message)
        {
        }
    }
}