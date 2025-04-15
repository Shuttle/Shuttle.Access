using System;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Esb;
using Shuttle.Recall.Sql.EventProcessing;
using Shuttle.Recall.Sql.Storage;
using Shuttle.Recall;
using Microsoft.Extensions.Logging;

namespace Shuttle.Access.Server.v1.MessageHandlers;

public class MonitorKeepAliveHandler : IMessageHandler<MonitorKeepAlive>
{
    private readonly ILogger<MonitorKeepAliveHandler> _logger;
    private readonly KeepAliveObserver _keepAliveObserver;
    private readonly IPrimitiveEventRepository _primitiveEventRepository;
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly SqlStorageOptions _sqlStorageOptions;
    private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;
    private readonly ServerOptions _serverOptions;

    public MonitorKeepAliveHandler(ILogger<MonitorKeepAliveHandler> logger, IOptions<ServerOptions> serverOptions, IOptions<SqlStorageOptions> sqlStorageOptions, IOptions<SqlEventProcessingOptions> sqlEventProcessingOptions, IDatabaseContextFactory databaseContextFactory, IPrimitiveEventRepository primitiveEventRepository, KeepAliveObserver keepAliveObserver)
    {
        _logger = Guard.AgainstNull(logger);
        _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);
        _sqlStorageOptions = Guard.AgainstNull(Guard.AgainstNull(sqlStorageOptions).Value);
        _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(sqlEventProcessingOptions).Value);
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository);
        _keepAliveObserver = Guard.AgainstNull(keepAliveObserver);
    }

    public async Task ProcessMessageAsync(IHandlerContext<MonitorKeepAlive> context)
    {
        long maxSequenceNumber;

        await using (_databaseContextFactory.Create(_sqlStorageOptions.ConnectionStringName))
        {
            maxSequenceNumber = await _primitiveEventRepository.GetMaxSequenceNumberAsync();
        }

        await using (var databaseContext = _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName))
        {
            if (await databaseContext.GetScalarAsync<int>(new Query($@"
IF EXISTS
(
    SELECT
        NULL
    FROM
        [{_sqlEventProcessingOptions.Schema}].[Projection]
    WHERE
        [SequenceNumber] < {maxSequenceNumber}
) 
    SELECT 1 
ELSE 
    SELECT 0
")) == 0)
            {
                _logger.LogDebug($"[keep-alive] : reset");

                await _keepAliveObserver.ResetAsync();

                return;
            }
        }

        var ignoreTillDate = DateTime.UtcNow.Add(_serverOptions.MonitorKeepAliveInterval);

        await context.SendAsync(new MonitorKeepAlive(), builder =>
        {
            builder.Local().Defer(ignoreTillDate);
        });

        _logger.LogDebug($"[keep-alive] : ignore till date = '{ignoreTillDate:O}'");
    }
}