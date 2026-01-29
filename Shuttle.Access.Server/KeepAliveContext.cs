using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Access.Server;

public class KeepAliveContext(IOptions<ServerOptions> serverOptions) : IKeepAliveContext
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ServerOptions _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);
    private bool _keepAliveSent;
    private DateTimeOffset _keepAliveSentAt = DateTimeOffset.MinValue;

    public async Task SentAsync()
    {
        await _lock.WaitAsync();

        try
        {
            _keepAliveSent = true;
            _keepAliveSentAt = DateTimeOffset.UtcNow;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<bool> GetShouldIgnoreAsync()
    {
        await _lock.WaitAsync();

        try
        {
            return _keepAliveSent || _keepAliveSentAt > DateTimeOffset.UtcNow.Subtract(_serverOptions.MonitorKeepAliveInterval);
        }
        finally
        {
            _lock.Release();
        }
    }

    public DateTimeOffset GetIgnoreTillDate()
    {
        return DateTime.UtcNow.Add(_serverOptions.MonitorKeepAliveInterval);
    }

    public async Task ResetAsync()
    {
        await _lock.WaitAsync();

        try
        {
            _keepAliveSent = false;
        }
        finally
        {
            _lock.Release();
        }
    }
}