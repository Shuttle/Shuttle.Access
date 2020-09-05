using System;

namespace Shuttle.Access
{
    public interface IAccessConfiguration
    {
        string ProviderName { get; }
        string ConnectionString { get; }
        TimeSpan SessionDuration { get; set; }
    }
}