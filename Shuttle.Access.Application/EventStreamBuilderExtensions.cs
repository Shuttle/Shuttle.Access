using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

public static class EventStreamBuilderExtensions
{
    extension(EventStreamBuilder eventStreamBuilder)
    {
        public EventStreamBuilder AddAuditIdentityName(string identityName)
        {
            eventStreamBuilder.AddHeader("AuditIdentityName", Guard.AgainstEmpty(identityName));
            return eventStreamBuilder;
        }
    }
}