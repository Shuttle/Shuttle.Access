using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shuttle.Mediator;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;

namespace Shuttle.Access.Application;

[SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection", Justification = "Schema and table names are from trusted configuration sources")]
public class MonitorKeepAliveParticipant(IOptions<SqlServerStorageOptions> sqlServerStorageOptions, SqlServerStorageDbContext sqlServerStorageDbContext, SqlServerEventProcessingDbContext sqlServerEventProcessingDbContext) : IParticipant<MonitorKeepAlive>
{
    public async Task HandleAsync(MonitorKeepAlive message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var maxSequenceNumber = await sqlServerStorageDbContext.Database
            .SqlQueryRaw<long?>($@"SELECT MAX(SequenceNumber) [Value] FROM [{sqlServerStorageOptions.Value.Schema}].[PrimitiveEvent]")
            .SingleAsync(cancellationToken) ?? 0;

        var nullSequenceNumberCount = await sqlServerStorageDbContext.Database
            .SqlQueryRaw<int>($@"SELECT COUNT(*) [Value] FROM [{sqlServerStorageOptions.Value.Schema}].[PrimitiveEvent] WHERE SequenceNumber IS NULL")
            .SingleAsync(cancellationToken);

        if (nullSequenceNumberCount != 0)
        {
            return;
        }

        var hasOutstandingProjections = await sqlServerEventProcessingDbContext.Database.SqlQueryRaw<int>($@"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT
                NULL
            FROM
                [{sqlServerStorageOptions.Value.Schema}].[Projection]
            WHERE
                [SequenceNumber] < {maxSequenceNumber}
        )
        THEN
            1
        ELSE
            0
    END [VALUE]
").SingleAsync(cancellationToken) == 1;

        if (!hasOutstandingProjections)
        {
            message.Reset();
        }
    }
}
