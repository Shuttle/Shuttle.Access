using System;
using System.Collections.Generic;
using System.Linq;
using Shuttle.Core.Contract;
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
            Save(eventStream, null);
        }

        public void Save(EventStream eventStream, Action<EventEnvelopeConfigurator> configurator)
        {
            Guard.AgainstNull(eventStream, nameof(eventStream));

            eventStream.Commit();
        }

        public void Remove(Guid id)
        {
            _eventStreams.Remove(id);
        }

        public T FindEvent<T>(Guid id, int index = -1) where T: class
        {
            var events = Get(id).GetEvents();

            try
            {
                if (index > -1)
                {
                    return (T)events.ElementAtOrDefault(index)?.Event;
                }

                var type = typeof(T);

                foreach (var domainEvent in events)
                {
                    if (domainEvent.Event.GetType() != type)
                    {
                        continue;
                    }

                    return (T)domainEvent.Event;
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }
    }
}