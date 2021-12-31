using System;

namespace Shuttle.Access.RestClient
{
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message)
        {
        }
    }
}