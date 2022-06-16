using System;

namespace Shuttle.Access
{
    public interface IAccessConnectionConfiguration
    {
        string ProviderName { get; }
        string ConnectionString { get; }
    }
}