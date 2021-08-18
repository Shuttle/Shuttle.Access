using System;

namespace Shuttle.Access.Application
{
    public interface IClientConfiguration
    {
        Uri Url { get; }
        string IdentityName { get; }
        string Password { get; }
    }
}