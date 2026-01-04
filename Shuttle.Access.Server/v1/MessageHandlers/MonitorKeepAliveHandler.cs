using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Hopper;
using Shuttle.Recall;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Server.v1.MessageHandlers;

[SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection", Justification = "Schema and table names are from trusted configuration sources")]
public class MonitorKeepAliveHandler(ILogger<MonitorKeepAliveHandler> logger, IOptions<ServerOptions> serverOptions, IOptions<SqlServerStorageOptions> sqlServerStorageOptions, IOptions<SqlServerEventProcessingOptions> sqlServerEventProcessingOptions, IDbContextFactory<SqlServerStorageDbContext> sqlServerStorageDbContextFactory, IDbContextFactory<SqlServerEventProcessingDbContext> sqlServerEventProcessingDbContext, IPrimitiveEventRepository primitiveEventRepository, KeepAliveObserver keepAliveObserver)
    : IMessageHandler<MonitorKeepAlive>
{
    private readonly KeepAliveObserver _keepAliveObserver = Guard.AgainstNull(keepAliveObserver);
    private readonly ILogger<MonitorKeepAliveHandler> _logger = Guard.AgainstNull(logger);
    private readonly IPrimitiveEventRepository _primitiveEventRepository = Guard.AgainstNull(primitiveEventRepository);
    private readonly ServerOptions _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);
    private readonly SqlServerEventProcessingOptions _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(sqlServerEventProcessingOptions).Value);
    private readonly IDbContextFactory<SqlServerEventProcessingDbContext> _sqlServerEventProcessingDbContext = Guard.AgainstNull(sqlServerEventProcessingDbContext);
    private readonly IDbContextFactory<SqlServerStorageDbContext> _sqlServerStorageDbContextFactory = Guard.AgainstNull(sqlServerStorageDbContextFactory);
    private readonly SqlServerStorageOptions _sqlServerStorageOptions = Guard.AgainstNull(Guard.AgainstNull(sqlServerStorageOptions).Value);

    public async Task ProcessMessageAsync(IHandlerContext<MonitorKeepAlive> context, CancellationToken cancellationToken = default)
    {
        long maxSequenceNumber;
        int nullSequenceNumberCount;

        await using (var dbContext = await _sqlServerStorageDbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            maxSequenceNumber = await dbContext.Database
                .SqlQueryRaw<long?>($@"SELECT MAX(SequenceNumber) [Value] FROM [{_sqlServerStorageOptions.Schema}].[PrimitiveEvent]")
                .SingleAsync(cancellationToken) ?? 0;

            nullSequenceNumberCount = await dbContext.Database
                .SqlQueryRaw<int>($@"SELECT COUNT(*) [Value] FROM [{_sqlServerStorageOptions.Schema}].[PrimitiveEvent] WHERE SequenceNumber IS NULL")
                .SingleAsync(cancellationToken);
        }

        if (nullSequenceNumberCount == 0)
        {
            await using (var dbContext = await _sqlServerEventProcessingDbContext.CreateDbContextAsync(cancellationToken))
            {
                if (await dbContext.Database.SqlQueryRaw<int>($@"
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
").SingleAsync(cancellationToken) == 0)
                {
                    _logger.LogDebug("[keep-alive] : reset");

                    await _keepAliveObserver.ResetAsync();

                    return;
                }
            }
        }

        var ignoreTillDate = DateTime.UtcNow.Add(_serverOptions.MonitorKeepAliveInterval);

        await context.SendAsync(new MonitorKeepAlive(), builder =>
        {
            builder.ToSelf().DeferUntil(ignoreTillDate);
        }, cancellationToken);

        _logger.LogDebug($"[keep-alive] : ignore till date = '{ignoreTillDate:O}'");
    }
}