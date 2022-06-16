using System;

namespace Shuttle.Access
{
    public interface IAccessSessionConfiguration
    {
        TimeSpan SessionDuration { get; set; }
    }
}