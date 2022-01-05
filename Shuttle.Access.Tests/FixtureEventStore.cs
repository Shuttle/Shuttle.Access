using System;
using System.Collections.Generic;
using Shuttle.Recall;

namespace Shuttle.Access.Tests
{
    public class FixtureEventStore : IEventStore
    {
        private readonly Dictionary<Guid, EventStream> _eventStreams = new();

        public EventStream CreateEventStream(Guid id)
        {
            if (_eventStreams.ContainsKey(id))
            {
                throw new DuplicateKeyException(id.ToString());
            }

            var result = new EventStream(id, new DefaultEventMethodInvoker(new EventMethodInvokerConfiguration()));

            _eventStreams.Add(id, result);

            return result;
        }

        public EventStream CreateEventStream()
        {
            return CreateEventStream(Guid.NewGuid());
        }

        public EventStream Get(Guid id)
        {
            return _eventStreams.ContainsKey(id) ? _eventStreams[id] : CreateEventStream(id);
        }

        public void Save(EventStream eventStream)
        {
        }

        public void Save(EventStream eventStream, Action<EventEnvelopeConfigurator> configurator)
        {
        }

        public void Remove(Guid id)
        {
            _eventStreams.Remove(id);
        }
    }
}