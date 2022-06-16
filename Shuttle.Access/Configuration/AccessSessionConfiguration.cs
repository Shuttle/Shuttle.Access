using System;

namespace Shuttle.Access
{
    public class AccessSessionConfiguration : IAccessSessionConfiguration
    {
        public AccessSessionConfiguration()
        {
            SessionDuration = TimeSpan.FromHours(1);
        }

        public TimeSpan SessionDuration { get; set; }
    }
}