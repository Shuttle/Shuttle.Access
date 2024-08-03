using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Tests
{
    public class FixtureEventStore : IEventStore
    {
        private readonly Dictionary<Guid, EventStream> _eventStreams = new();
        private long _sequenceNumber = 1;

        private EventStream CreateEventStream(Guid id)
        {
            if (_eventStreams.ContainsKey(id))
            {
                throw new DuplicateKeyException(id.ToString());
            }

            var result = new EventStream(id, new DefaultEventMethodInvoker(new EventMethodInvokerConfiguration()));

            _eventStreams.Add(id, result);

            return result;
        }

        public T FindEvent<T>(Guid id, int index = -1) where T: class
        {
            var events = Get(id).GetEvents(EventStream.EventRegistrationType.All);

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

        public EventStream Get(Guid id, Action<EventStreamBuilder> builder = null)
        {
            return _eventStreams.ContainsKey(id) ? _eventStreams[id] : CreateEventStream(id);
        }

        public async Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder> builder = null)
        {
            return await Task.FromResult(Get(id, builder));
        }

        public long Save(EventStream eventStream, Action<EventStreamBuilder> builder = null)
        {
            Guard.AgainstNull(eventStream, nameof(eventStream)).Commit();

            return _sequenceNumber++;
        }

        public async ValueTask<long> SaveAsync(EventStream eventStream, Action<EventStreamBuilder> builder = null)
        {
            return await Task.FromResult(Save(eventStream, builder));
        }

        public void Remove(Guid id, Action<EventStreamBuilder> builder = null)
        {
            _eventStreams.Remove(id);
        }

        public async Task RemoveAsync(Guid id, Action<EventStreamBuilder> builder = null)
        {
            Remove(id, builder);
            await Task.CompletedTask;
        }
    }
}