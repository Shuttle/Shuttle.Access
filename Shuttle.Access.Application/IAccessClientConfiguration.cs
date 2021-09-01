using System;

namespace Shuttle.Access.Application
{
    public interface IAccessClientConfiguration
    {
        Uri Url { get; }
        string IdentityName { get; }
        string Password { get; }
    }
}