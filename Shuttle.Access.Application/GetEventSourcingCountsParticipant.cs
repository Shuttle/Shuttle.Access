using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shuttle.Mediator;
using Shuttle.Recall.SqlServer.EventProcessing;
using Shuttle.Recall.SqlServer.Storage;
using System.Diagnostics.CodeAnalysis;
using Shuttle.Recall;

namespace Shuttle.Access.Application;

[SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection", Justification = "Schema and table names are from trusted configuration sources")]
public class GetEventSourcingCountsParticipant(IOptions<SqlServerStorageOptions> sqlServerStorageOptions, IEventProcessorConfiguration eventProcessorConfiguration, SqlServerStorageDbContext sqlServerStorageDbContext, SqlServerEventProcessingDbContext sqlServerEventProcessingDbContext) : IParticipant<GetEventSourcingCounts>
{
    public async Task HandleAsync(GetEventSourcingCounts message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.MaximumSequenceNumber = await sqlServerStorageDbContext.Database
            .SqlQueryRaw<long?>($@"SELECT MAX(SequenceNumber) [Value] FROM [{sqlServerStorageOptions.Value.Schema}].[PrimitiveEvent]")
            .SingleAsync(cancellationToken) ?? 0;

        var projectionCount = await sqlServerEventProcessingDbContext.Database
            .SqlQueryRaw<int>($@"SELECT COUNT(*) [Value] FROM [{sqlServerStorageOptions.Value.Schema}].[Projection]")
            .SingleAsync(cancellationToken);

        message.HasUnsequencedPrimitiveEvents = await sqlServerStorageDbContext.Database.SqlQueryRaw<int>($@"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT
                NULL
            FROM 
                [{sqlServerStorageOptions.Value.Schema}].[PrimitiveEvent] 
            WHERE 
                SequenceNumber IS NULL
        )
        THEN 
            1
        ELSE 
            0
    END [VALUE]
").SingleAsync(cancellationToken) > 0;

        message.HasWaitingProjections = projectionCount != eventProcessorConfiguration.Projections.Count() ||
                                        await sqlServerEventProcessingDbContext.Database.SqlQueryRaw<int>($@"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT
                NULL
            FROM 
                [{sqlServerStorageOptions.Value.Schema}].[Projection]
            WHERE 
                [SequenceNumber] < {message.MaximumSequenceNumber}
        )
        THEN 
            1
        ELSE 
            0
    END [VALUE]
").SingleAsync(cancellationToken) == 1;
    }
}