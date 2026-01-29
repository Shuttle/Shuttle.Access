using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Hopper;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Server.v1.MessageHandlers;

[SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection", Justification = "Schema and table names are from trusted configuration sources")]
public class MonitorKeepAliveHandler(ILogger<MonitorKeepAliveHandler> logger, IOptions<ServerOptions> serverOptions, IOptions<SqlServerStorageOptions> sqlServerStorageOptions, IOptions<SqlServerEventProcessingOptions> sqlServerEventProcessingOptions, SqlServerStorageDbContext sqlServerStorageDbContext, SqlServerEventProcessingDbContext sqlServerEventProcessingDbContext, IKeepAliveContext keepAliveContext)
    : IMessageHandler<MonitorKeepAlive>
{
    private readonly IKeepAliveContext _keepAliveContext = Guard.AgainstNull(keepAliveContext);
    private readonly ILogger<MonitorKeepAliveHandler> _logger = Guard.AgainstNull(logger);
    private readonly ServerOptions _serverOptions = Guard.AgainstNull(Guard.AgainstNull(serverOptions).Value);
    private readonly SqlServerEventProcessingOptions _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(sqlServerEventProcessingOptions).Value);
    private readonly SqlServerEventProcessingDbContext _sqlServerEventProcessingDbContext = Guard.AgainstNull(sqlServerEventProcessingDbContext);
    private readonly SqlServerStorageDbContext _sqlServerStorageDbContext = Guard.AgainstNull(sqlServerStorageDbContext);
    private readonly SqlServerStorageOptions _sqlServerStorageOptions = Guard.AgainstNull(Guard.AgainstNull(sqlServerStorageOptions).Value);

    public async Task ProcessMessageAsync(IHandlerContext<MonitorKeepAlive> context, CancellationToken cancellationToken = default)
    {
        var maxSequenceNumber = await _sqlServerStorageDbContext.Database
            .SqlQueryRaw<long?>($@"SELECT MAX(SequenceNumber) [Value] FROM [{_sqlServerStorageOptions.Schema}].[PrimitiveEvent]")
            .SingleAsync(cancellationToken) ?? 0;

        var nullSequenceNumberCount = await _sqlServerStorageDbContext.Database
            .SqlQueryRaw<int>($@"SELECT COUNT(*) [Value] FROM [{_sqlServerStorageOptions.Schema}].[PrimitiveEvent] WHERE SequenceNumber IS NULL")
            .SingleAsync(cancellationToken);

        if (nullSequenceNumberCount == 0)
        {
            if (await _sqlServerEventProcessingDbContext.Database.SqlQueryRaw<int>($@"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT
                NULL
            FROM 
                [{_sqlEventProcessingOptions.Schema}].[Projection]
            WHERE 
                [SequenceNumber] < {maxSequenceNumber}
        )
        THEN 
            1
        ELSE 
            0
    END [VALUE]
").SingleAsync(cancellationToken) == 0)
            {
                _logger.LogDebug("[keep-alive] : reset");

                await _keepAliveContext.ResetAsync();

                return;
            }
        }

        var ignoreTillDate = DateTime.UtcNow.Add(_serverOptions.MonitorKeepAliveInterval);

        await context.SendAsync(new MonitorKeepAlive(), builder =>
        {
            builder.ToSelf().DeferUntil(ignoreTillDate);
        }, cancellationToken);

        _logger.LogDebug("[keep-alive] : ignore till date = '{IgnoreTillDate:O}'", ignoreTillDate);
    }
}