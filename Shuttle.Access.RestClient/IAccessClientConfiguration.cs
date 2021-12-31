using System;

namespace Shuttle.Access.RestClient
{
    public interface IAccessClientConfiguration
    {
        Uri BaseAddress { get; }
        string IdentityName { get; }
        string Password { get; }
    }
}