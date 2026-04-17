using Microsoft.Extensions.Options;
using Shuttle.Contract;
using Shuttle.Recall;

namespace Shuttle.Access.Tests;

public class FixtureEventStore : IEventStore
{
    private readonly Dictionary<Guid, EventStream> _eventStreams = new();

    public async Task<EventStream> GetAsync(Guid id, Action<EventStreamBuilder>? builder = null, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(Get(id, builder));
    }

    public Task<IEnumerable<EventEnvelope>> SaveAsync(EventStream eventStream, Action<EventStreamBuilder>? builder = null, CancellationToken cancellationToken = default)
    {
        Save(eventStream, builder);

        return Task.FromResult<IEnumerable<EventEnvelope>>([]);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _eventStreams.Remove(id);

        await Task.CompletedTask;
    }

    private EventStream CreateEventStream(Guid id)
    {
        if (_eventStreams.ContainsKey(id))
        {
            throw new DuplicateKeyException(id.ToString());
        }

        var result = new EventStream(id, new EventMethodInvoker(Options.Create(new RecallOptions())));

        _eventStreams.Add(id, result);

        return result;
    }

    public T? FindEvent<T>(Guid id, Func<T, bool>? specification = null, int index = -1) where T : class
    {
        var events = Get(id).GetEvents(EventStream.EventRegistrationType.All);

        try
        {
            if (index > -1)
            {
                var candidate = events.ElementAtOrDefault(index)?.Event as T;

                if (candidate == null)
                {
                    return null;
                }

                if (specification == null || specification(candidate))
                {
                    return candidate;
                }

                return null;
            }

            foreach (var domainEvent in events)
            {
                if (domainEvent.Event is not T typed)
                {
                    continue;
                }

                if (specification != null && !specification(typed))
                {
                    continue;
                }

                return typed;
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    public EventStream Get(Guid id, Action<EventStreamBuilder>? builder = null)
    {
        return _eventStreams.TryGetValue(id, out var stream) ? stream : CreateEventStream(id);
    }

    public void Save(EventStream eventStream, Action<EventStreamBuilder>? builder = null)
    {
        Guard.AgainstNull(eventStream).Commit();
    }
}