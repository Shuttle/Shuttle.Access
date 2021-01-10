using System;

namespace Shuttle.Access.Api
{
    public interface IClientConfiguration
    {
        Uri Url { get; }
        string IdentityName { get; }
        string Password { get; }
    }
}